using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Industry;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Sale.SaleOrder;
using Application.Services.SearchRequestModels.Report;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Accounts.Customers;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Report.Sales;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleOrder;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Threading.Tasks;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Sale.SaleOrders
{
    public class SaleOrderService : BaseService<SaleOrder, SaleOrderCreationDto, SaleOrderUpdateDto, SaleOrderRequestModel, SaleOrderViewModel>, ISaleOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IAccountService _accountService;
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly ITenantService _tenantService;
        private readonly IIndustryProfile _industry;

        public SaleOrderService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, IAccountService accountService, ICustomerService customerService,
            INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext, ITenantService tenantService, IIndustryProfile industry) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _accountService = accountService;
            _customerService = customerService;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _tenantService = tenantService;
            _industry = industry;
        }

        public async Task<SaleOrderAggregatorModel> PrepareSaleOrderAggregatorModel(SaleOrderRequestModel saleOrderRequest)
        {
            var saleOrderQueryable = _unitOfWork.Repository<SaleOrder>().TableNoTracking().Where(saleOrderRequest.GetExpression());
            return new SaleOrderAggregatorModel
            {
                AggregatorSubtotal = await saleOrderQueryable.SumAsync(x => x.Subtotal),
                AggregatorTotalPercentageDiscountAmount = await saleOrderQueryable.SumAsync(x => x.TotalPercentageDiscountAmount),
                AggregatorDiscount = await saleOrderQueryable.SumAsync(x => x.Discount),
                AggregatorOfferDiscount = await saleOrderQueryable.SumAsync(x => x.OfferDiscount),
                AggregatorOtherDiscount = await saleOrderQueryable.SumAsync(x => x.OtherDiscount),
                AggregatorTotal = await saleOrderQueryable.SumAsync(x => x.Total),
                AggregatorTransportationCost = await saleOrderQueryable.SumAsync(x => x.TransportationCost),
                AggregatorDepoCharge = await saleOrderQueryable.SumAsync(x => x.DepoCharge),
                AggregatorNetTotal = await saleOrderQueryable.SumAsync(x => x.NetTotal),
            };
        }

        public new async Task<Guid> AddAsync(SaleOrderCreationDto saleOrderCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            saleOrderCreationDto.OrderDate = saleOrderCreationDto.OrderDate.ToLocal();
            saleOrderCreationDto.DeliveryDate = saleOrderCreationDto.DeliveryDate.ToLocal();
            var saleOrder = _mapper.Map<SaleOrder>(saleOrderCreationDto);
            var count = _unitOfWork.Repository<SaleOrder>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            saleOrder.Id = Guid.NewGuid();
            saleOrder.SaleOrderNo = "SO" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            saleOrder.FinancialYearId = financialYear.Id;
            saleOrder.Status = (int)SaleOrderStatus.Pending;
            //fill credit limit  and limit availed 
            var customer = await _unitOfWork.Repository<Account>().FindAsync(saleOrderCreationDto.CustomerId);
            var balance = await _accountService.CalculateBalanceByAccountId(saleOrderCreationDto.CustomerId, Guid.Parse(AccountTypeConstants.CurrentAsset), financialYear.Id, null, null);
            saleOrder.CreditLimit = customer.CustomerCreditLimit;
            saleOrder.LimitAvailed = balance;
            saleOrder.CustomerMarketingOfficerId = customer.CustomerMarketingOfficerId;
            //...
            //insert discount history
            if (saleOrderCreationDto.DiscountProductWiseId.HasValue)
            {
                await _unitOfWork.Repository<DiscountProductWiseUsageHistory>().AddAsync(new DiscountProductWiseUsageHistory
                {
                    Id = Guid.NewGuid(),
                    SaleOrderId = saleOrder.Id,
                    DiscountProductWiseId = saleOrderCreationDto.DiscountProductWiseId.Value,

                });
            }

            if (saleOrderCreationDto.CustomerWiseProductDiscountId.HasValue)
            {
                await _unitOfWork.Repository<CustomerWiseProductDiscountUsageHistory>().AddAsync(new CustomerWiseProductDiscountUsageHistory
                {
                    Id = Guid.NewGuid(),
                    SaleOrderId = saleOrder.Id,
                    CustomerWiseProductDiscountId = saleOrderCreationDto.CustomerWiseProductDiscountId.Value
                });
            }
            //.....
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var customerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                                                       .TableNoTracking().SingleOrDefaultAsync(x => x.CustomerId == saleOrder.CustomerId && x.IsActive && x.Status == (int)CustomerWiseProductDiscountStatus.Approved);
            var totalOrderedQty = saleOrder.SaleOrderDetails.Sum(x => x.Quantity);
            foreach (var item in saleOrder.SaleOrderDetails)
            {
                item.Id = Guid.NewGuid();
                item.SaleOrderId = saleOrder.Id;
                item.OtherDiscountPerUnit = saleOrder.OtherDiscount / totalOrderedQty;
                item.DepoChargePerUnit = saleOrder.DepoCharge / totalOrderedQty;
                item.TransportationCostPerUnit = saleOrder.TransportationCost / totalOrderedQty;

                if (tenantData.BusinessType == 2)
                {
                    if (customerWiseProductDiscount is not null)
                    {
                        var discountDetail = await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>()
                                                          .TableNoTracking()
                                                          .SingleOrDefaultAsync(x => x.CustomerWiseProductDiscountId == customerWiseProductDiscount.Id
                                                          && x.ProductId == item.ProductId);
                        if (discountDetail is null)
                        {
                            var product = await _unitOfWork.Repository<Product>().TableNoTracking().FirstAsync(x => x.Id == item.ProductId);
                            throw new BadRequestException($"No Customer Wise Product Discount found for Product: {product.Name}");
                        }
                    }
                    else throw new BadRequestException("No active and approved Customer Wise Product Discount found for this customer");
                }

                await _unitOfWork.Repository<SaleOrderDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<SaleOrder>().AddAsync(saleOrder);
            //update sale quotation
            if (!string.IsNullOrEmpty(saleOrderCreationDto.QuotationNo))
            {
                var dbSaleQuotation = await _unitOfWork.Repository<SaleQuotation>().TableNoTracking().SingleOrDefaultAsync(x => x.QuotationNo == saleOrderCreationDto.QuotationNo);
                if (dbSaleQuotation is null) throw new NotFoundResultException("Sale Quotation Not Found");
                dbSaleQuotation.Status = (int)SaleQuotationStatus.Sale_Order_Created;
                await _unitOfWork.Repository<SaleQuotation>().UpdateAsync(dbSaleQuotation);
            }
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, saleOrder.Id
                , "/sales/sales-order/" + saleOrder.Id, Permissions.SaleOrders.Check,
                "Sale Order " + saleOrder.SaleOrderNo + " is ready for Check", (int)SaleOrderStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return saleOrder.Id;
        }

        public new async Task<Guid> UpdateAsync(SaleOrderUpdateDto saleOrderUpdateDto)
        {
            var dbSaleOrder = await _unitOfWork.Repository<SaleOrder>().FindAsync(saleOrderUpdateDto.Id);
            //when date updated,convert it to local date
            if (saleOrderUpdateDto.OrderDate.Equals(dbSaleOrder.OrderDate) == false)
            {
                saleOrderUpdateDto.OrderDate = saleOrderUpdateDto.OrderDate.ToLocal();
                //insert Discount Product Wise history
                if (saleOrderUpdateDto.DiscountProductWiseId.HasValue)
                {
                    await _unitOfWork.Repository<DiscountProductWiseUsageHistory>().DeleteAsync(x => x.SaleOrderId == saleOrderUpdateDto.Id);
                    await _unitOfWork.Repository<DiscountProductWiseUsageHistory>().AddAsync(new DiscountProductWiseUsageHistory
                    {
                        Id = Guid.NewGuid(),
                        SaleOrderId = saleOrderUpdateDto.Id,
                        DiscountProductWiseId = saleOrderUpdateDto.DiscountProductWiseId.Value
                    });
                }
                else await _unitOfWork.Repository<DiscountProductWiseUsageHistory>().DeleteAsync(x => x.SaleOrderId == saleOrderUpdateDto.Id);
            }
            if (saleOrderUpdateDto.DeliveryDate.Equals(dbSaleOrder.DeliveryDate) == false)
            {
                saleOrderUpdateDto.DeliveryDate = saleOrderUpdateDto.DeliveryDate.ToLocal();
            }
            var saleOrder = _mapper.Map(saleOrderUpdateDto, dbSaleOrder);
            if (dbSaleOrder.Status == (int)SaleOrderStatus.Pending || dbSaleOrder.Status == (int)SaleOrderStatus.Checked)
            {
                await _unitOfWork.Repository<SaleOrder>().UpdateAsync(saleOrder);
                //insert discount history
                if (saleOrderUpdateDto.CustomerWiseProductDiscountId.HasValue)
                {
                    await _unitOfWork.Repository<CustomerWiseProductDiscountUsageHistory>().DeleteAsync(x => x.SaleOrderId == saleOrder.Id);
                    await _unitOfWork.Repository<CustomerWiseProductDiscountUsageHistory>().AddAsync(new CustomerWiseProductDiscountUsageHistory
                    {
                        Id = Guid.NewGuid(),
                        SaleOrderId = saleOrder.Id,
                        CustomerWiseProductDiscountId = saleOrderUpdateDto.CustomerWiseProductDiscountId.Value
                    });
                }
                else await _unitOfWork.Repository<CustomerWiseProductDiscountUsageHistory>().DeleteAsync(x => x.SaleOrderId == saleOrderUpdateDto.Id);
                //.......
                var totalOrderedQty = saleOrder.SaleOrderDetails.Sum(x => x.Quantity);
                foreach (var item in saleOrder.SaleOrderDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.SaleOrderId = saleOrder.Id;
                        item.OtherDiscountPerUnit = saleOrder.OtherDiscount / totalOrderedQty;
                        item.DepoChargePerUnit = saleOrder.DepoCharge / totalOrderedQty;
                        item.TransportationCostPerUnit = saleOrder.TransportationCost / totalOrderedQty;
                        await _unitOfWork.Repository<SaleOrderDetail>().AddAsync(item);
                    }
                    else
                    {
                        item.OtherDiscountPerUnit = saleOrder.OtherDiscount / totalOrderedQty;
                        item.DepoChargePerUnit = saleOrder.DepoCharge / totalOrderedQty;
                        item.TransportationCostPerUnit = saleOrder.TransportationCost / totalOrderedQty;
                        await _unitOfWork.Repository<SaleOrderDetail>().UpdateAsync(item);
                    }
                }
                //Delete for SaleOrderDetails
                if (!string.IsNullOrEmpty(saleOrderUpdateDto.DeletedSaleOrderDetailIds))
                {
                    foreach (var id in saleOrderUpdateDto.DeletedSaleOrderDetailIds.Split(',').Where(x => x != ""))
                    {
                        var saleOrderDetail = await _unitOfWork.Repository<SaleOrderDetail>().FindAsync(new Guid(id));
                        saleOrderDetail.Deleted = true;
                        await _unitOfWork.Repository<SaleOrderDetail>().UpdateAsync(saleOrderDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return saleOrder.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var saleOrder = await _unitOfWork.Repository<SaleOrder>().FindAsync(x => x.Id == id, x => x.Include(x => x.SaleOrderDetails));
            if (saleOrder == null) throw new NotFoundResultException("SaleOrder Not Found With this Id");
            if (saleOrder.Status == (int)SaleOrderStatus.Pending)
            {
                foreach (var item in saleOrder.SaleOrderDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<SaleOrderDetail>().UpdateAsync(item);
                }
                saleOrder.Deleted = true;
                await _unitOfWork.Repository<SaleOrder>().UpdateAsync(saleOrder);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return saleOrder.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var saleOrders = _unitOfWork.Repository<SaleOrder>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await saleOrders.Where(x => x.Status == (int)SaleOrderStatus.Pending).CountAsync(),
                CheckedCount = await saleOrders.Where(x => x.Status == (int)SaleOrderStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbSaleOrder = await _unitOfWork.Repository<SaleOrder>().FindAsync(id);
            if (dbSaleOrder.Status == (int)SaleOrderStatus.Pending)
            {
                dbSaleOrder.Status = (int)SaleOrderStatus.Checked;
                dbSaleOrder.CheckedBy = _workContext.GetUserName();
                //update SaleOrder
                await _unitOfWork.Repository<SaleOrder>().UpdateAsync(dbSaleOrder);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSaleOrder.Id
                       && x.Status == (int)SaleOrderStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbSaleOrder.Id
                    , "/sales/sales-order/" + dbSaleOrder.Id, Permissions.SaleOrders.Approve,
                    "Sale Order " + dbSaleOrder.SaleOrderNo + " is ready for Approval", (int)SaleOrderStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Sale Order Status has already been checked by " + dbSaleOrder?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbSaleOrderInfo = await _unitOfWork.Repository<SaleOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.CustomerId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbSaleOrderInfo == null)
                throw new NotFoundResultException("Sale Order not found.");

            if (financialYear.Id != dbSaleOrderInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Sale Order does not belong to the current financial year.");

            var dbDeliveryNotesQueryable = _unitOfWork.Repository<DeliveryNote>().TableNoTracking().Where(x => x.CustomerId == dbSaleOrderInfo.CustomerId
            && x.Status == (int)DeliveryNoteStatus.Approved);
            var partialDeliveredSaleOrders = _unitOfWork.Repository<SaleOrder>().TableNoTracking().Include(x => x.SaleOrderDetails).ThenInclude(x => x.Product).
                Where(x => x.CustomerId == dbSaleOrderInfo.CustomerId && x.Status == (int)SaleOrderStatus.Item_Partially_Delivered);

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<SaleOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)SaleOrderStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)SaleOrderStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Sale Order already approved by {dbSaleOrderInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbSaleOrder = await _unitOfWork.Repository<SaleOrder>().FindAsync(id);

            dbSaleOrder.Status = (int)SaleOrderStatus.Approved;
            dbSaleOrder.ApprovedBy = approvedBy;

            if (dbDeliveryNotesQueryable.Any() || partialDeliveredSaleOrders.Any())
            {
                await CheckCustomerCreditLimitAndBalance(dbSaleOrder, dbDeliveryNotesQueryable, partialDeliveredSaleOrders);
            }
            await ChangeMoneyReceiptStatus(dbSaleOrder);
            //update SaleOrder
            await _unitOfWork.Repository<SaleOrder>().UpdateAsync(dbSaleOrder);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSaleOrder.Id
                   && x.Status == (int)SaleOrderStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }

        private async Task CheckCustomerCreditLimitAndBalance(SaleOrder dbSaleOrder, IQueryable<DeliveryNote> dbDeliveryNotesQueryable, IQueryable<SaleOrder> partialDeliveredSaleOrders)
        {
            var transitAmountInPartialDeliveredSaleOrder = 0m;
            if (partialDeliveredSaleOrders.Any())
            {
                foreach (var saleOrder in partialDeliveredSaleOrders)
                {
                    foreach (var item in saleOrder.SaleOrderDetails)
                    {
                        var quantity = item.Quantity - _industry.Uom.ToSellable(item.DeliveredPrimaryQuantity, item.Product);
                        var rate = item.NetRate - item.OtherDiscountPerUnit + item.TransportationCostPerUnit + item.DepoChargePerUnit;
                        transitAmountInPartialDeliveredSaleOrder += (quantity * rate);
                    }
                }
            }
            var customerBalance = await _customerService.FindCustomerCreditLimitAndBalance(dbSaleOrder.CustomerId);
            // Calculate the available credit after deducting the balance (considering delivery notes those invoice not created yet and transit Amount In Partial Delivered Sale Order)
            var availableCredit = customerBalance.creditLimit - customerBalance.balance - dbDeliveryNotesQueryable.Sum(x => x.NetTotal) - transitAmountInPartialDeliveredSaleOrder;
            // Check if netTotal exceeds available credit
            if (availableCredit < 0M) throw new BadRequestException($"You don't have available credit limit/balance !!");
        }

        public virtual async Task<bool> CloseAsync(Guid id)
        {
            var dbSaleOrder = await _unitOfWork.Repository<SaleOrder>().FindAsync(id);
            if (dbSaleOrder.Status == (int)SaleOrderStatus.Item_Partially_Delivered || dbSaleOrder.Status == (int)SaleOrderStatus.Partially_Invoiced)
            {
                dbSaleOrder.Status = (int)SaleOrderStatus.Closed;
                await _unitOfWork.Repository<SaleOrder>().UpdateAsync(dbSaleOrder);
                bool isClose = await _unitOfWork.SaveChangesAsync();
                return isClose;
            }
            else throw new BadRequestException("You won't be able to close this Sale Order");
        }

        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Load minimal data (no tracking)
            var dbInfo = await _unitOfWork.Repository<SaleOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.SaleOrderNo })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Sale Order not found.");
            var isDeliveryNoteExists = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking().AnyAsync(x => x.SaleOrderNo == dbInfo.SaleOrderNo);
            if (isDeliveryNoteExists) throw new BadRequestException("Unpost Denied: Delivery Note exists for this Sale Order.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Sale Order does not belong to the current financial year.");
            if (dbInfo.Status != fromStatus) throw new BadRequestException("Sale Order status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)SaleOrderStatus.Checked && dbInfo.Status != (int)SaleOrderStatus.Approved)
                throw new BadRequestException("Only Checked or Approved Sale Order can be unposted.");

            var newStatus = dbInfo.Status == (int)SaleOrderStatus.Approved
                ? (int)SaleOrderStatus.Checked
                : (int)SaleOrderStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<SaleOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus));
            // .SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Sale Order status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbSaleOrder = await _unitOfWork.Repository<SaleOrder>().FindAsync(id);

            dbSaleOrder.Status = newStatus;
            // dbSaleOrder.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)SaleOrderStatus.Checked:
                    dbSaleOrder.CheckedBy = "";
                    break;
                case (int)SaleOrderStatus.Approved:
                    await ReverseMoneyReceiptStatus(dbSaleOrder);
                    dbSaleOrder.ApprovedBy = "";
                    break;
            }

            await _unitOfWork.Repository<SaleOrder>().UpdateAsync(dbSaleOrder);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbSaleOrder.Status;
        }

        public async Task<SaleOrderViewModel> GetByIdAsync(Guid id)
        {
            var saleOrder = await _unitOfWork.Repository<SaleOrder>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Customer).Include(x => x.Store)
                .Include(x => x.SaleOrderDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (saleOrder == null) throw new NotFoundResultException("Sale Order Not Found With this Id");
            return _mapper.Map<SaleOrderViewModel>(saleOrder);
        }

        private async Task ChangeMoneyReceiptStatus(SaleOrder saleOrder)
        {
            if (!string.IsNullOrWhiteSpace(saleOrder.MoneyReceiptNo))
            {
                var arr = saleOrder.MoneyReceiptNo.Split(',');
                foreach (var code in arr)
                {
                    var moneyReceipt = await _unitOfWork.Repository<ReceivePayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == code);
                    moneyReceipt!.Status = (int)ReceivePaymentStatus.Occupied;
                    await _unitOfWork.Repository<ReceivePayment>().UpdateAsync(moneyReceipt);
                }

            }
        }

        private async Task ReverseMoneyReceiptStatus(SaleOrder saleOrder)
        {
            if (!string.IsNullOrWhiteSpace(saleOrder.MoneyReceiptNo))
            {
                var arr = saleOrder.MoneyReceiptNo.Split(',');
                foreach (var code in arr)
                {
                    var moneyReceipt = await _unitOfWork.Repository<ReceivePayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == code);
                    moneyReceipt!.Status = (int)ReceivePaymentStatus.Approved;
                    await _unitOfWork.Repository<ReceivePayment>().UpdateAsync(moneyReceipt);
                }

            }
        }
        public async Task<List<SalesOrderItemWithoutDeliveryItemViewModel>> GetSaleOrderedDataWithoutDeliveryItemAsync(SalesOrderItemRequestModel request)
        {
            var saleOrderQueryable = _unitOfWork.Repository<SaleOrder>().TableNoTracking().Include(x => x.Store).Include(x => x.Customer)
                .ThenInclude(x => x.CustomerMarketingOfficer).Include(x => x.SaleOrderDetails).AsQueryable();
            var deliveryNoteQueryable = _unitOfWork.Repository<DeliveryNote>().TableNoTracking().Where(x => x.Status >= (int)DeliveryNoteStatus.Approved).Include(x => x.Store).Include(x => x.Customer)
                .ThenInclude(x => x.CustomerMarketingOfficer).Include(x => x.DeliveryNoteDetails).AsQueryable();
            if (request.FromDate.HasValue)
            {
                saleOrderQueryable = saleOrderQueryable.Where(x => x.OrderDate.Date >= request.FromDate.Value.Date);
                deliveryNoteQueryable = deliveryNoteQueryable.Where(x => x.DeliveryDate.Date >= request.FromDate!.Value.Date);
            }
            if (request.ToDate.HasValue)
            {
                saleOrderQueryable = saleOrderQueryable.Where(x => x.OrderDate.Date <= request.ToDate.Value.Date);
                deliveryNoteQueryable = deliveryNoteQueryable.Where(x => x.DeliveryDate.Date <= request.ToDate!.Value.Date);
            }
            if (request.CustomerId.HasValue)
            {
                saleOrderQueryable = saleOrderQueryable.Where(x => x.CustomerId == request.CustomerId).OrderByDescending(x => x.SaleOrderNo);
                deliveryNoteQueryable = deliveryNoteQueryable.Where(x => x.CustomerId == request.CustomerId);
            }
            if (request.StoreId.HasValue)
            {
                saleOrderQueryable = saleOrderQueryable.Where(x => x.StoreId == request.StoreId);
                deliveryNoteQueryable = deliveryNoteQueryable.Where(x => x.StoreId == request.StoreId);
            }
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                saleOrderQueryable = saleOrderQueryable.Where(x => x.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId);
                deliveryNoteQueryable = deliveryNoteQueryable.Where(x => x.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId);
            }
            if (request.CustomerZoneId.HasValue)
            {
                saleOrderQueryable = saleOrderQueryable.Where(x => x.Customer.CustomerZoneId == request.CustomerZoneId);
                deliveryNoteQueryable = deliveryNoteQueryable.Where(x => x.Customer.CustomerZoneId == request.CustomerZoneId);
            }
            if (request.CustomerAreaId.HasValue)
            {
                saleOrderQueryable = saleOrderQueryable.Where(x => x.Customer.CustomerAreaId == request.CustomerAreaId);
                deliveryNoteQueryable = deliveryNoteQueryable.Where(x => x.Customer.CustomerAreaId == request.CustomerAreaId);
            }

            var salesOrderItems = await saleOrderQueryable.SelectMany(order => order.SaleOrderDetails.Select(detail => new SalesOrderItemViewModel
            {
                StoreName = order.Store.Name,
                CustomerName = order.Customer.Name,
                SaleOrderNo = order.SaleOrderNo,
                SaleOrderId = detail.SaleOrderId,
                SaleOrderDetailId = detail.Id,
                ProductId = detail.ProductId,
                ProductName = detail.Product.Name,
                UnitName = detail.Product.MeasurementUnit.Name,
                PrimaryQuantity = order.Status == (int)SaleOrderStatus.Closed ? detail.DeliveredPrimaryQuantity : detail.PrimaryQuantity,
                Quantity = order.Status == (int)SaleOrderStatus.Closed ? (detail.DeliveredPrimaryQuantity * detail.Product.BagWeight) : detail.Quantity,
                CancelPrimaryQuantity = order.Status == (int)SaleOrderStatus.Closed ? detail.PrimaryQuantity - detail.DeliveredPrimaryQuantity : 0,
                CancelQuantity = order.Status == (int)SaleOrderStatus.Closed ? detail.Quantity - (detail.DeliveredPrimaryQuantity * detail.Product.BagWeight) : 0,
                Rate = detail.Rate + detail.TransportationCostPerUnit + detail.DepoChargePerUnit - (detail.CashDiscountPerUnit + detail.InvoiceDiscountPerUnit + detail.SpecialDiscountPerUnit + detail.OtherDiscountPerUnit + detail.OfferDiscountPerUnit)
            })).ToListAsync();

            var deliveryNoteItems = await deliveryNoteQueryable.SelectMany(order => order.DeliveryNoteDetails.Select(detail => new DeliveryNoteItemViewModel
            {
                StoreName = order.Store.Name,
                CustomerName = order.Customer.Name,
                SaleOrderNo = order.SaleOrderNo,
                DeliveryNoteId = detail.DeliveryNoteId,
                DeliveryNoteDetailId = detail.Id,
                ProductId = detail.ProductId,
                ProductName = detail.Product.Name,
                UnitName = detail.Product.MeasurementUnit.Name,
                DeliveryPrimaryQuantity = detail.DeliveryPrimaryQuantity,
                DeliveryQuantity = detail.DeliveryQuantity,
                DeliveryRate = detail.Rate + detail.TransportationCostPerUnit + detail.DepoChargePerUnit - (detail.CashDiscountPerUnit + detail.InvoiceDiscountPerUnit + detail.SpecialDiscountPerUnit + detail.OtherDiscountPerUnit + detail.OfferDiscountPerUnit)
            })).ToListAsync();

            var query = from salesOrderItem in salesOrderItems
                        join deliveryNoteItem in deliveryNoteItems
                        on new { salesOrderItem.SaleOrderNo, salesOrderItem.ProductId } equals new { deliveryNoteItem.SaleOrderNo, deliveryNoteItem.ProductId } into deliveries // Perform a left join
                        from delivery in deliveries.DefaultIfEmpty()    // Include orders even if no deliveries exist
                        group delivery by new
                        {
                            salesOrderItem.StoreName,
                            salesOrderItem.CustomerName,
                            salesOrderItem.SaleOrderNo,
                            salesOrderItem.SaleOrderId,
                            salesOrderItem.SaleOrderDetailId,
                            salesOrderItem.ProductId,
                            salesOrderItem.ProductName,
                            salesOrderItem.UnitName,
                            salesOrderItem.PrimaryQuantity,
                            salesOrderItem.Quantity,
                            salesOrderItem.CancelPrimaryQuantity,
                            salesOrderItem.CancelQuantity,
                            salesOrderItem.Rate
                        } into g
                        select new SalesOrderItemWithoutDeliveryItemViewModel
                        {
                            StoreName = g.Key.StoreName,
                            CustomerName = g.Key.CustomerName,
                            SaleOrderNo = g.Key.SaleOrderNo,
                            SaleOrderDetailId = g.Key.SaleOrderDetailId,
                            ProductId = g.Key.ProductId,
                            ProductName = g.Key.ProductName,
                            UnitName = g.Key.UnitName,
                            OrderedPrimaryQuantity = g.Key.PrimaryQuantity,
                            OrderedQuantity = g.Key.Quantity,
                            DeliveredPrimaryQuantity = g.Sum(del => del != null ? del.DeliveryPrimaryQuantity : 0), // Sum valid deliveries
                            DeliveredQuantity = g.Sum(del => del != null ? del.DeliveryQuantity : 0), // Sum valid deliveries
                            CancelPrimaryQuantity = g.Key.CancelPrimaryQuantity,
                            CancelQuantity = g.Key.CancelQuantity,
                            Rate = g.Key.Rate
                        };
            if (!request.FromDate.HasValue) query = query.Where(x => x.OrderedPrimaryQuantity != x.DeliveredPrimaryQuantity);
            return query.OrderByDescending(x => x.SaleOrderNo).ToList();
        }
    }
}
