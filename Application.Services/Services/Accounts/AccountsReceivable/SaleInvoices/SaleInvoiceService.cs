using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Industry;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.SearchRequestModels.Report;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Accounts.Customers;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.SaleInvoice;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Report.Sales;
using Application.Services.ViewModels.Sale;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Dynamic;
using System.Globalization;
using System.Text;
using static iTextSharp.text.pdf.AcroFields;

namespace Application.Services.Services.Accounts.AccountsReceivable.SaleInvoices
{
    public class SaleInvoiceService : BaseService<SaleInvoice, SaleInvoiceCreationDto, SaleInvoiceUpdateDto, SaleInvoiceRequestModel, SaleInvoiceViewModel>, ISaleInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ICustomerService _customerService;
        private readonly IMailService _mailService;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _configuration;
        private readonly ITenantService _tenantService;
        private readonly IIndustryProfile _industry;

        public SaleInvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
            IHubContext<BroadcastHub, IHubClient> hubContext, IAccountService accountService, ICustomerService customerService
            , IMailService mailService, ISmsService smsService, IConfiguration configuration, ITenantService tenantService, IIndustryProfile industry) : base(unitOfWork, mapper, workContext)
        {
            _industry = industry;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
            _customerService = customerService;
            _mailService = mailService;
            _smsService = smsService;
            _configuration = configuration;
            _tenantService = tenantService;
        }

        public async Task<SaleInvoiceAggregatorModel> PrepareSaleInvoiceAggregatorModel(SaleInvoiceRequestModel saleInvoiceRequest)
        {
            var saleInvoiceQueryable = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(saleInvoiceRequest.GetExpression());
            return new SaleInvoiceAggregatorModel
            {
                AggregatorSubtotal = await saleInvoiceQueryable.SumAsync(x => x.Subtotal),
                AggregatorTotalPercentageDiscountAmount = await saleInvoiceQueryable.SumAsync(x => x.TotalPercentageDiscountAmount),
                AggregatorDiscount = await saleInvoiceQueryable.SumAsync(x => x.Discount),
                AggregatorOfferDiscount = await saleInvoiceQueryable.SumAsync(x => x.OfferDiscount),
                AggregatorOtherDiscount = await saleInvoiceQueryable.SumAsync(x => x.OtherDiscount),
                AggregatorTotal = await saleInvoiceQueryable.SumAsync(x => x.Total),
                AggregatorTransportationCost = await saleInvoiceQueryable.SumAsync(x => x.TransportationCost),
                AggregatorDepoCharge = await saleInvoiceQueryable.SumAsync(x => x.DepoCharge),
                AggregatorNetTotal = await saleInvoiceQueryable.SumAsync(x => x.NetTotal),
                AggregatorPaid = await saleInvoiceQueryable.SumAsync(x => x.Paid),

            };
        }

        public async Task<SaleInvoiceViewModel> GetByIdAsync(Guid id)
        {
            var saleInvoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Customer)
                .Include(x => x.SaleInvoiceDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (saleInvoice == null) throw new NotFoundResultException("Sale Invoice Not Found With this Id");
            return _mapper.Map<SaleInvoiceViewModel>(saleInvoice);
        }

        public new async Task<AddUpdateResponseModel> AddAsync(SaleInvoiceCreationDto saleInvoiceCreationDto)
        {
            if (_industry.Sales.OneInvoicePerSaleOrder)
            {
                var isSaleInvoiceExistAgainstSO = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().AnyAsync(x => x.SaleOrderNo == saleInvoiceCreationDto.SaleOrderNo);
                if (isSaleInvoiceExistAgainstSO) throw new BadRequestException($"You have already made an invoice using this Sale Order {saleInvoiceCreationDto.SaleOrderNo}");
            }
            var financialYear = _workContext.GetCurrentFinancialYear();
            if (!string.IsNullOrWhiteSpace(saleInvoiceCreationDto.DeliveryNoteNo))
            {
                foreach (var deliveryNoteNo in saleInvoiceCreationDto.DeliveryNoteNo!.Split(","))
                {
                    var saleInvoiceExists = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().AnyAsync(x => x.DeliveryNoteNo.Contains(deliveryNoteNo));
                    if (saleInvoiceExists) throw new BadRequestException("You have already created sale invoice using this delivery note# " + deliveryNoteNo);
                }
            }

            saleInvoiceCreationDto.InvoiceDate = saleInvoiceCreationDto.InvoiceDate.ToLocal();
            var saleInvoice = _mapper.Map<SaleInvoice>(saleInvoiceCreationDto);
            var count = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            saleInvoice.Id = Guid.NewGuid();
            saleInvoice.InvoiceNo = "SI" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            saleInvoice.FinancialYearId = financialYear.Id;
            saleInvoice.Status = (int)SaleInvoiceStatus.Pending;
            //auto approve
            saleInvoice.Status = (int)SaleInvoiceStatus.Approved;
            saleInvoice.ApprovedBy = _workContext.GetUserName();

            //balance amount
            var customerInfo = await _customerService.FindCustomerCreditLimitAndBalance(saleInvoice.CustomerId);
            saleInvoice.Balance = customerInfo.balance;
            saleInvoice.CustomerMarketingOfficerId = customerInfo.customerMarketingOfficerId;
            var totalVat = 0M;
            var cogs_Total = 0.0M;
            foreach (var item in saleInvoice.SaleInvoiceDetails)
            {
                item.Id = Guid.NewGuid();
                item.SaleInvoiceId = saleInvoice.Id;
                if (string.IsNullOrWhiteSpace(saleInvoice.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(saleInvoice.SaleOrderNo))
                {
                    await IncreaseSaleOrderDetailDeliveredQuantityBySaleInvoiceQuantity(item);
                    decimal cogs = await StockOut(saleInvoice, item);
                    cogs_Total += cogs;
                }
                //Vat Calculation start
                var dbProduct = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
                if (dbProduct == null) throw new NotFoundResultException("Product Not Found");
                item.VatPercentage = dbProduct.VatPercentage;
                totalVat += VatCalculator.CalculateVat(dbProduct.VatPercentage, (item.Quantity * item.Rate));
                //Vat Calculation End
                //update sale invoice detail
                var customerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                                                       .TableNoTracking().SingleOrDefaultAsync(x => x.CustomerId == saleInvoice.CustomerId && x.IsActive);
                if (customerWiseProductDiscount is not null)
                {
                    var discountDetail = await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>()
                                                      .TableNoTracking()
                                                      .SingleOrDefaultAsync(x => x.CustomerWiseProductDiscountId == customerWiseProductDiscount.Id
                                                      && x.ProductId == item.ProductId);
                    if (discountDetail is not null)
                    {
                        item.MonthlyDiscountPerUnit = discountDetail.MonthlyDiscount;
                        item.YearlyDiscountPerUnit = discountDetail.YearlyDiscount;
                        item.TargetDiscountPerUnit = discountDetail.TargetDiscount;
                    }
                    else
                    {
                        item.MonthlyDiscountPerUnit = 0;
                        item.YearlyDiscountPerUnit = 0;
                        item.TargetDiscountPerUnit = 0;
                    }
                }
                else
                {
                    item.MonthlyDiscountPerUnit = 0;
                    item.YearlyDiscountPerUnit = 0;
                    item.TargetDiscountPerUnit = 0;
                }

                //end
                await _unitOfWork.Repository<SaleInvoiceDetail>().AddAsync(item);
            }
            saleInvoice.TotalVat = totalVat;
            saleInvoice.AvailableReceivable = saleInvoice.NetTotal;
            await _unitOfWork.Repository<SaleInvoice>().AddAsync(saleInvoice);
            //when invoiced,then change delivery note status
            if (!string.IsNullOrWhiteSpace(saleInvoice.DeliveryNoteNo))
            {
                foreach (var deliveryNoteNo in saleInvoice.DeliveryNoteNo.Split(","))
                {
                    var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking().SingleOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);
                    deliveryNote!.Status = (int)DeliveryNoteStatus.Invoice_Generated;
                    await _unitOfWork.Repository<DeliveryNote>().UpdateAsync(deliveryNote);
                }

            }

            //send notification
            //await _notificationService.SendNotificationAsync(_unitOfWork, saleInvoice.Id
            //    , "/sales/sale-invoice/" + saleInvoice.Id, Permissions.SaleInvoices.Check,
            //    "Sale Invoice " + saleInvoice.InvoiceNo + " is ready for Approval", (int)SaleInvoiceStatus.Pending);
            await AutoApproveAsync(saleInvoice, cogs_Total);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            return new AddUpdateResponseModel { Id = saleInvoice.Id, Code = saleInvoice.InvoiceNo, Status = saleInvoice.Status };
        }

        public new async Task<AddUpdateResponseModel> UpdateAsync(SaleInvoiceUpdateDto saleInvoiceUpdateDto)
        {
            var dbSaleInvoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(saleInvoiceUpdateDto.Id);
            //when date updated,convert it to local date
            if (saleInvoiceUpdateDto.InvoiceDate.Equals(dbSaleInvoice.InvoiceDate) == false)
            {
                saleInvoiceUpdateDto.InvoiceDate = saleInvoiceUpdateDto.InvoiceDate.ToLocal();
            }
            if (dbSaleInvoice == null) throw new NotFoundResultException("SaleInvoice Not Found With this Id");
            var saleInvoice = _mapper.Map(saleInvoiceUpdateDto, dbSaleInvoice);
            if (dbSaleInvoice.Status == (int)SaleInvoiceStatus.Pending || dbSaleInvoice.Status == (int)SaleInvoiceStatus.Checked)
            {

                var totalVat = 0M;
                foreach (var item in saleInvoice.SaleInvoiceDetails)
                {
                    //Vat Calculation start
                    var dbProduct = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
                    if (dbProduct == null) throw new NotFoundResultException("Product Not Found");
                    item.VatPercentage = dbProduct.VatPercentage;
                    totalVat += VatCalculator.CalculateVat(dbProduct.VatPercentage, (item.Quantity * item.Rate));
                    //Vat Calculation End
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.SaleInvoiceId = saleInvoice.Id;
                        await _unitOfWork.Repository<SaleInvoiceDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<SaleInvoiceDetail>().UpdateAsync(item);
                }
                saleInvoice.TotalVat = totalVat;
                await _unitOfWork.Repository<SaleInvoice>().UpdateAsync(saleInvoice);
                //Delete for SaleInvoiceDetails
                if (!string.IsNullOrEmpty(saleInvoiceUpdateDto.DeletedSaleInvoiceDetailIds))
                {
                    foreach (var id in saleInvoiceUpdateDto.DeletedSaleInvoiceDetailIds.Split(',').Where(x => x != ""))
                    {
                        var saleInvoiceDetail = await _unitOfWork.Repository<SaleInvoiceDetail>().FindAsync(new Guid(id));
                        saleInvoiceDetail.Deleted = true;
                        await _unitOfWork.Repository<SaleInvoiceDetail>().UpdateAsync(saleInvoiceDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return new AddUpdateResponseModel { Id = saleInvoice.Id, Code = saleInvoice.InvoiceNo, Status = saleInvoice.Status };
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var saleInvoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(x => x.Id == id, x => x.Include(x => x.SaleInvoiceDetails));
            if (saleInvoice == null) throw new NotFoundResultException("Sale Invoice Not Found With this Id");
            if (saleInvoice.Status == (int)SaleInvoiceStatus.Pending)
            {
                foreach (var item in saleInvoice.SaleInvoiceDetails.ToList())
                    await _unitOfWork.Repository<SaleInvoiceDetail>().DeleteAsync(item);
                await _unitOfWork.Repository<SaleInvoice>().DeleteAsync(saleInvoice);
                //reverted delivery note status
                if (!string.IsNullOrWhiteSpace(saleInvoice.DeliveryNoteNo))
                {
                    foreach (var deliveryNoteNo in saleInvoice.DeliveryNoteNo.Split(","))
                    {
                        var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking().SingleOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);
                        deliveryNote!.Status = (int)DeliveryNoteStatus.Approved;
                        await _unitOfWork.Repository<DeliveryNote>().UpdateAsync(deliveryNote);
                    }

                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");
            return saleInvoice.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var saleInvoices = _unitOfWork.Repository<SaleInvoice>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await saleInvoices.Where(x => x.Status == (int)SaleInvoiceStatus.Pending).CountAsync(),
                CheckedCount = await saleInvoices.Where(x => x.Status == (int)SaleInvoiceStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbSaleInvoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(id);
            if (dbSaleInvoice.Status == (int)SaleInvoiceStatus.Pending)
            {
                dbSaleInvoice.Status = (int)SaleInvoiceStatus.Checked;
                dbSaleInvoice.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<SaleInvoice>().UpdateAsync(dbSaleInvoice);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSaleInvoice.Id
                       && x.Status == (int)SaleInvoiceStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbSaleInvoice.Id
                    , "/sales/sale-invoice/" + dbSaleInvoice.Id, Permissions.SaleInvoices.Approve,
                    "Sale Invoice " + dbSaleInvoice.InvoiceNo + " is ready for Approval", (int)SaleInvoiceStatus.Checked);
                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Sale Invoice Status has already been checked by " + dbSaleInvoice?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<SaleInvoice>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Sale Invoice Not Found!");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Sale Invoice does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<SaleInvoice>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)SaleInvoiceStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)SaleInvoiceStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Sale Invoice already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbSaleInvoice = await _unitOfWork.Repository<SaleInvoice>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.Customer).Include(x => x.SaleInvoiceDetails));

            dbSaleInvoice.Status = (int)SaleInvoiceStatus.Approved;
            dbSaleInvoice.ApprovedBy = approvedBy;

            //paid amount
            var paidAmount = 0m;
            if (!string.IsNullOrWhiteSpace(dbSaleInvoice.MoneyReceiptNo))
            {
                var arr = dbSaleInvoice.MoneyReceiptNo.Split(',');
                foreach (var code in arr)
                {
                    var moneyReceipt = _unitOfWork.Repository<ReceivePayment>().TableNoTracking().SingleOrDefault(x => x.Code == code);
                    if (moneyReceipt is null) throw new Exception("Money Receipt Not Found !!");
                    paidAmount += moneyReceipt.TotalAmount;
                }
            }
            dbSaleInvoice.Paid = paidAmount;
            //balance amount
            var customerInfo = await _customerService.FindCustomerCreditLimitAndBalance(dbSaleInvoice.CustomerId);
            dbSaleInvoice.Balance = customerInfo.balance;
            //update sale invoice detail
            var cogs_Total = 0.0M;
            foreach (var item in dbSaleInvoice.SaleInvoiceDetails)
            {
                if (string.IsNullOrWhiteSpace(dbSaleInvoice.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(dbSaleInvoice.SaleOrderNo))
                {
                    await IncreaseSaleOrderDetailDeliveredQuantityBySaleInvoiceQuantity(item);
                    decimal cogs = await StockOut(dbSaleInvoice, item);
                    cogs_Total += cogs;
                }

                var customerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                                                       .TableNoTracking().SingleOrDefaultAsync(x => x.CustomerId == dbSaleInvoice.CustomerId && x.IsActive && x.Status == (int)CustomerWiseProductDiscountStatus.Approved);
                if (customerWiseProductDiscount is not null)
                {
                    var discountDetail = await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>()
                                                      .TableNoTracking()
                                                      .SingleOrDefaultAsync(x => x.CustomerWiseProductDiscountId == customerWiseProductDiscount.Id
                                                      && x.ProductId == item.ProductId);
                    if (discountDetail is not null)
                    {
                        item.MonthlyDiscountPerUnit = discountDetail.MonthlyDiscount;
                        item.YearlyDiscountPerUnit = discountDetail.YearlyDiscount;
                        item.TargetDiscountPerUnit = discountDetail.TargetDiscount;
                    }
                    else
                    {
                        item.MonthlyDiscountPerUnit = 0;
                        item.YearlyDiscountPerUnit = 0;
                        item.TargetDiscountPerUnit = 0;
                    }
                }
                else
                {
                    item.MonthlyDiscountPerUnit = 0;
                    item.YearlyDiscountPerUnit = 0;
                    item.TargetDiscountPerUnit = 0;
                }
                await _unitOfWork.Repository<SaleInvoiceDetail>().UpdateAsync(item);
            }
            if (string.IsNullOrWhiteSpace(dbSaleInvoice.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(dbSaleInvoice.SaleOrderNo))
            {
                var saleOrder = await _unitOfWork.Repository<SaleOrder>().TableNoTracking().Include(x => x.SaleOrderDetails).SingleOrDefaultAsync(x => x.SaleOrderNo == dbSaleInvoice.SaleOrderNo);
                if (saleOrder is null) throw new NotFoundResultException("Sale Order not found associated with this Sale Invoice " + dbSaleInvoice.SaleOrderNo);
                await ChangeSaleOrderStatus(dbSaleInvoice, saleOrder);
            }
            await InsertTransaction(dbSaleInvoice, cogs_Total);
            //update SaleInvoice
            await _unitOfWork.Repository<SaleInvoice>().UpdateAsync(dbSaleInvoice);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSaleInvoice.Id
                   && x.Status == (int)SaleInvoiceStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }

        public virtual async Task AutoApproveAsync(SaleInvoice saleInvoice, decimal cogs_Total)
        {
            if (string.IsNullOrWhiteSpace(saleInvoice.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(saleInvoice.SaleOrderNo))
            {
                var saleOrder = await _unitOfWork.Repository<SaleOrder>().TableNoTracking().Include(x => x.SaleOrderDetails).SingleOrDefaultAsync(x => x.SaleOrderNo == saleInvoice.SaleOrderNo);
                if (saleOrder is null) throw new NotFoundResultException("Sale Order not found associated with this Sale Invoice " + saleInvoice.SaleOrderNo);
                await ChangeSaleOrderStatus(saleInvoice, saleOrder);
            }

            await InsertTransaction(saleInvoice, cogs_Total);
        }

        private async Task IncreaseSaleOrderDetailDeliveredQuantityBySaleInvoiceQuantity(SaleInvoiceDetail saleInvoiceDetail)
        {
            if (saleInvoiceDetail.SaleOrderDetailId.HasValue)
            {
                var saleOrderDetail = await _unitOfWork.Repository<SaleOrderDetail>().FindAsync(x => x.Id == saleInvoiceDetail.SaleOrderDetailId.Value);
                var orderedBagQuantity = saleOrderDetail.PrimaryQuantity + saleOrderDetail.PrimaryBonusQuantity;
                saleOrderDetail.DeliveredPrimaryQuantity += saleInvoiceDetail.PrimaryQuantity;
                saleOrderDetail.DeliveredPrimaryBonusQuantity += saleInvoiceDetail.PrimaryBonusQuantity;
                var totalDeliveredBagQuantity = saleOrderDetail.DeliveredPrimaryQuantity + saleOrderDetail.DeliveredPrimaryBonusQuantity;
                if (totalDeliveredBagQuantity > orderedBagQuantity) throw new BadRequestException("The invoice quantity for " + saleOrderDetail.Product.Name + " exceeds the ordered quantity.!!");
                await _unitOfWork.Repository<SaleOrderDetail>().UpdateAsync(saleOrderDetail);
            }
            else throw new NotFoundResultException("Associated Sale Order Not Found!!");
        }

        private async Task ReduceSaleOrderDetailDeliveredQuantityBySaleInvoiceQuantity(SaleInvoiceDetail saleInvoiceDetail)
        {
            if (saleInvoiceDetail.SaleOrderDetailId.HasValue)
            {
                var saleOrderDetail = await _unitOfWork.Repository<SaleOrderDetail>().FindAsync(x => x.Id == saleInvoiceDetail.SaleOrderDetailId.Value);
                saleOrderDetail.DeliveredPrimaryQuantity -= saleInvoiceDetail.PrimaryQuantity;
                saleOrderDetail.DeliveredPrimaryBonusQuantity -= saleInvoiceDetail.PrimaryBonusQuantity;
                await _unitOfWork.Repository<SaleOrderDetail>().UpdateAsync(saleOrderDetail);
            }
        }

        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Load minimal data (no tracking)
            var dbInfo = await _unitOfWork.Repository<SaleInvoice>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.Paid, x.InvoiceNo, x.SaleOrderNo })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Sale Invoice Not Found!");
            if (dbInfo.Paid > 0) throw new BadRequestException("Unpost Denied: Payment has already been made against this invoice.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Sale Invoice does not belong to the current financial year.");

            var isSaleReturnExists = await _unitOfWork.Repository<SaleReturn>().TableNoTracking().AnyAsync(x => x.InvoiceNo.Contains(dbInfo.InvoiceNo));
            if (isSaleReturnExists) throw new BadRequestException("Unpost Denied: A sales return exists for this invoice.");
            var saleOrder = await _unitOfWork.Repository<SaleOrder>().TableNoTracking().Include(x => x.SaleOrderDetails).SingleOrDefaultAsync(x => x.SaleOrderNo == dbInfo.SaleOrderNo);
            if (saleOrder is null) throw new NotFoundResultException("Sale Order not found associated with this Sale Invoice " + dbInfo.SaleOrderNo);

            if (dbInfo.Status != fromStatus) throw new BadRequestException("Sale Invoice status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)SaleInvoiceStatus.Checked && dbInfo.Status != (int)SaleInvoiceStatus.Approved)
                throw new BadRequestException("Only Checked or Approved Sale Invoice can be unposted.");

            var newStatus = dbInfo.Status == (int)SaleInvoiceStatus.Approved
                ? (int)SaleInvoiceStatus.Checked
                : (int)SaleInvoiceStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<SaleInvoice>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Sale Invoice status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbSaleInvoice = await _unitOfWork.Repository<SaleInvoice>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.Customer).Include(x => x.SaleInvoiceDetails));

            dbSaleInvoice.Status = newStatus;
            dbSaleInvoice.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)SaleInvoiceStatus.Checked:
                    dbSaleInvoice.CheckedBy = "";
                    break;
                case (int)SaleInvoiceStatus.Approved:
                    if (_industry.Sales.BlockInvoiceUnpostWhenReceiptExists)
                    {
                        var isReceivePaymentExistAgainstSI = await _unitOfWork.Repository<ReceivePaymentAgainstSaleSaleInvoiceMapping>().TableNoTracking().AnyAsync(x => x.SaleInvoiceId == dbSaleInvoice.Id);
                        if (isReceivePaymentExistAgainstSI) throw new BadRequestException("Unposting blocked: money receipt already exists. Please unpost the receipt first.");
                    }
                    dbSaleInvoice.ApprovedBy = "";
                    dbSaleInvoice.Paid = 0;
                    dbSaleInvoice.Balance = 0;
                    await DeleteTransaction(dbSaleInvoice.InvoiceNo);
                    if (string.IsNullOrWhiteSpace(dbSaleInvoice.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(dbSaleInvoice.SaleOrderNo))
                    {
                        foreach (var item in dbSaleInvoice.SaleInvoiceDetails)
                        {
                            await ReduceSaleOrderDetailDeliveredQuantityBySaleInvoiceQuantity(item);
                        }
                        await ReverseStock(dbSaleInvoice);
                        await ChangeSaleOrderStatusWhenUnpost(dbSaleInvoice, saleOrder);
                    }
                    break;
            }

            await _unitOfWork.Repository<SaleInvoice>().UpdateAsync(dbSaleInvoice);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbSaleInvoice.Status;
        }

        public virtual async Task<bool> SendToCustomerAsync(Guid id)
        {
            var saleInvoice = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == id);
            if (string.IsNullOrWhiteSpace(saleInvoice!.Customer.ContactNo))
            {
                throw new BadRequestException("Customer Contact No Not Found");
            }

            //send email
            //if (!string.IsNullOrWhiteSpace(saleInvoice!.Customer.Email))
            //{

            //    var mailRequest = new MailRequest()
            //    {
            //        ToEmail = saleInvoice.Customer.Email,
            //        Subject = "Your APFEED Invoice Is Here",
            //        Body = meassage
            //    };
            //    await _mailService.SendEmailFromDefaultEmailAddressAsync(mailRequest);
            //}
            //send sms
            bool isSuccess = false;
            var isSmsSendAllowed = _configuration.GetValue<bool>("SmsSend:Allowed");
            if (isSmsSendAllowed)
            {
                var SmsSendBaseUrl = _configuration.GetValue<string>("SmsSend:BaseUrl");
                var meassage = $"Thanks For Purchasing Feed From AP FEED {Environment.NewLine}" +
                   $"Details:{SmsSendBaseUrl}/si/{saleInvoice.Id}";
                await _smsService.Send(saleInvoice.Customer.ContactNo, meassage);
                saleInvoice!.Status = (int)SaleInvoiceStatus.SentToCustomer;
                await _unitOfWork.Repository<SaleInvoice>().UpdateAsync(saleInvoice);
                isSuccess = await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Sms Send not Allowed!!");


            return isSuccess;

        }

        private (decimal cogs, List<Stock> stocks) CalculateCOGS(List<int> transactionTypes, Product product, Guid finishedGoodsStoreId, int sellQuantity)
        {
            List<Stock> stocks = new();
            var neededItems = sellQuantity;
            var itemsCalculated = 0.0M;
            var currentIndex = 0;
            var COGS_total = 0.0M;
            var stockList = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => transactionTypes.Contains(x.TransactonType) && x.StoreId == finishedGoodsStoreId && x.ProductId == product.Id && x.AvailableQty != 0)
                     .OrderBy(x => x.StockInDate).ToArray();
            if (!stockList.Any()) throw new NotFoundResultException($"{product.Name} is not available in stock.");
            var availableStock = stockList.Sum(x => x.AvailableQty);
            if (availableStock < neededItems) throw new BadRequestException($"Available stock quantity for {product.Name} is {availableStock.ToString("#,##0.000")} and needed quantity is {neededItems.ToString("#,##0.000")}");
            while (neededItems != itemsCalculated)
            {
                var stock = stockList[currentIndex];
                if (stock.AvailableQty > (neededItems - itemsCalculated))
                {
                    var used = (neededItems - itemsCalculated);
                    COGS_total = COGS_total + used * (stock.InRate + stock.InTransportCost);
                    stock.AvailableQty = stock.AvailableQty - used;
                    itemsCalculated = itemsCalculated + used;
                }
                else if (stock.AvailableQty == (neededItems - itemsCalculated))
                {
                    COGS_total = COGS_total + stock.AvailableQty * (stock.InRate + stock.InTransportCost);
                    itemsCalculated += stock.AvailableQty;
                    stock.AvailableQty = 0;
                }
                else if (stock.AvailableQty < (neededItems - itemsCalculated))
                {
                    COGS_total = COGS_total + stock.AvailableQty * (stock.InRate + stock.InTransportCost);
                    itemsCalculated += stock.AvailableQty;
                    stock.AvailableQty = 0;
                }

                stocks.Add(stock);
                currentIndex++;
            }

            return (COGS_total, stocks);
        }
        private async Task<decimal> StockOut(SaleInvoice saleInvoice, SaleInvoiceDetail item)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var deliveryQty = item.Quantity + item.BonusQuantity;
            var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
            var (cogs, stocks) = CalculateCOGS(new List<int> { (int)TransactonType.Production, (int)TransactonType.TransferReceive, (int)TransactonType.SaleReturn, (int)TransactonType.StockAdjustment_Plus, (int)TransactonType.Purchase }, product, saleInvoice.StoreId, deliveryQty);
            foreach (var stock in stocks)
            {
                await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
            }
            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.Sale,
                InventoryTypeId = product.InventoryTypeId,
                ProductTypeId = product.ProductTypeId,
                ProductId = item.ProductId,
                StoreId = saleInvoice.StoreId,
                OutQty = deliveryQty,
                OutRate = cogs / deliveryQty,
                BatchNo = string.Join(",", stocks.Select(x => x.BatchNo)),
                TransactionId = saleInvoice.Id,
                StockInDate = stocks.First().StockInDate,
                TransactionDate = saleInvoice.InvoiceDate,
                Remark = "Sale Invoice#" + saleInvoice.InvoiceNo,
                FinancialYearId = financialYear.Id
            });
            return cogs;
        }

        private async Task ReverseStock(SaleInvoice saleInvoice)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.Sale && x.TransactionId == saleInvoice.Id).ToListAsync();
            if (stocks.Any())
            {
                foreach (var stock in stocks)
                {
                    var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == stock.ProductId);
                    await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                    {
                        Id = Guid.NewGuid(),
                        TransactonType = (product.IsPurchaseProduct && product.IsSaleProduct) || product.IsSaleProduct
                        ? (int)TransactonType.Purchase
                        : (int)TransactonType.Production,
                        InventoryTypeId = stock.InventoryTypeId,
                        ProductTypeId = stock.ProductTypeId,
                        ProductId = stock.ProductId,
                        StoreId = stock.StoreId,
                        AvailableQty = stock.OutQty,
                        InQty = stock.OutQty,
                        InRate = stock.OutRate,
                        BatchNo = stock.BatchNo,
                        TransactionId = saleInvoice.Id,
                        StockInDate = stock.StockInDate,
                        TransactionDate = DateTime.Now,
                        Remark = "Reverse Stock for Sale Invoice#" + saleInvoice.InvoiceNo + " Unposted",
                        IsUnpostedEntry = true,
                        FinancialYearId = financialYear.Id
                    });
                }
                await _unitOfWork.Repository<Stock>().DeleteAsync(stocks);
            }
        }
        private async Task ChangeSaleOrderStatus(SaleInvoice saleInvoice, SaleOrder saleOrder)
        {
            if (!string.IsNullOrWhiteSpace(saleInvoice.SaleOrderNo))
            {
                var sumOrderedPrimaryQuantity = saleOrder!.SaleOrderDetails.Sum(x => x.PrimaryQuantity + x.PrimaryBonusQuantity);
                var totalInvoicedPrimaryQuantity = saleInvoice!.SaleInvoiceDetails.Sum(x => x.PrimaryQuantity + x.PrimaryBonusQuantity);
                if (totalInvoicedPrimaryQuantity > sumOrderedPrimaryQuantity) throw new BadRequestException("Invoice Qty exceeded the Ordered Qty!!");
                if (sumOrderedPrimaryQuantity == totalInvoicedPrimaryQuantity)
                    saleOrder!.Status = (int)SaleOrderStatus.Fully_Invoiced;
                else saleOrder!.Status = (int)SaleOrderStatus.Partially_Invoiced;
                saleOrder.SaleOrderDetails = null;
                await _unitOfWork.Repository<SaleOrder>().UpdateAsync(saleOrder);
            }
        }
        private async Task ChangeSaleOrderStatusWhenUnpost(SaleInvoice saleInvoice, SaleOrder saleOrder)
        {
            saleOrder!.Status = (int)SaleOrderStatus.Approved;
            saleOrder.SaleOrderDetails = null;
            await _unitOfWork.Repository<SaleOrder>().UpdateAsync(saleOrder);
        }
        private async Task InsertTransaction(SaleInvoice saleInvoice, decimal cogs_Total)
        {
            StringBuilder sb = new StringBuilder();
            var transactionQty = 0;
            var transactionQtyValue = 0m;
            var sl = 0;
            var totalMonthlyDiscount = 0m;
            var totalYearlyDiscount = 0m;
            var totalTargetDiscount = 0m;
            foreach (var item in saleInvoice!.SaleInvoiceDetails)
            {
                totalMonthlyDiscount += (item.Quantity * item.MonthlyDiscountPerUnit);
                totalYearlyDiscount += (item.Quantity * item.YearlyDiscountPerUnit);
                totalTargetDiscount += (item.Quantity * item.TargetDiscountPerUnit);
                sl++;
                var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
                var measurementUnit = await _unitOfWork.Repository<MeasurementUnit>().FindAsync(x => x.Id == product.MeasurementUnitId);
                transactionQty += item.Quantity;
                transactionQtyValue = saleInvoice.NetTotal;
                TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                sb.Append(sl);
                sb.Append(". ");
                sb.Append(textInfo.ToTitleCase(product?.Name?.ToLower() ?? ""));
                sb.Append(" ");
                sb.Append(item?.Quantity);
                sb.Append(textInfo.ToTitleCase(measurementUnit?.Name?.ToLower() ?? ""));
                sb.Append(" tp:");
                sb.Append(item?.Rate);
                sb.Append(" comm:");
                sb.Append(item?.DiscountPerUnit);
                sb.Append(" net:");
                sb.Append(item?.NetRate);
                sb.Append(";");
            }
            if (saleInvoice.NetTotal == (saleInvoice.Total + saleInvoice.TransportationCost + saleInvoice.DepoCharge))
            {
                StringBuilder contraAccountNamesForCredit = new();
                StringBuilder contraAccountIdsForCredit = new();
                StringBuilder contraAccountNamesForDebit = new();
                StringBuilder contraAccountIdsForDebit = new();
                var customerAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == saleInvoice.CustomerId).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForCredit.Append(customerAccountName);
                contraAccountIdsForCredit.Append(saleInvoice.CustomerId.ToString());
                if (totalMonthlyDiscount > 0m)
                {
                    var monthlyDiscountAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.MonthlyDiscount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForCredit.Append(", " + monthlyDiscountAccountName);
                    contraAccountIdsForCredit.Append(", " + AccountHeadConstants.MonthlyDiscount);
                }
                if (totalYearlyDiscount > 0m)
                {
                    var yearlyDiscountAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.YearlyDiscount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForCredit.Append(", " + yearlyDiscountAccountName);
                    contraAccountIdsForCredit.Append(", " + AccountHeadConstants.YearlyDiscount);
                }
                if (totalTargetDiscount > 0m)
                {
                    var targetDiscountAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.TargetDiscount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForCredit.Append(", " + targetDiscountAccountName);
                    contraAccountIdsForCredit.Append(", " + AccountHeadConstants.TargetDiscount);
                }
                //sales account credit
                var salesAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.Sales.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForDebit.Append(salesAccountName);
                contraAccountIdsForDebit.Append(AccountHeadConstants.Sales);
                if (saleInvoice.TotalVat > 0M)
                {
                    var vatOnSalesPayableAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.VatOnSalesPayable.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", " + vatOnSalesPayableAccountName);
                    contraAccountIdsForDebit.Append(", " + AccountHeadConstants.VatOnSalesPayable);
                }
                if (saleInvoice.TransportationCost != 0m)
                {
                    var carryingExpenses_SalesAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.CarryingExpenses_Sales.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", " + carryingExpenses_SalesAccountName);
                    contraAccountIdsForDebit.Append(", " + AccountHeadConstants.CarryingExpenses_Sales);
                }
                if (saleInvoice.DepoCharge > 0m)
                {
                    var depoChargeAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.DepotCharge.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", " + depoChargeAccountName);
                    contraAccountIdsForDebit.Append(", " + AccountHeadConstants.DepotCharge);
                }
                var totalDiscountBooking = totalMonthlyDiscount + totalYearlyDiscount + totalTargetDiscount;
                if (totalDiscountBooking > 0m)
                {
                    var commissionPayableAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.CommissionPayable.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", " + commissionPayableAccountName);
                    contraAccountIdsForDebit.Append(", " + AccountHeadConstants.CommissionPayable);
                }
                if (string.IsNullOrWhiteSpace(saleInvoice.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(saleInvoice.SaleOrderNo))
                {
                    var cogs_StandardAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.COGS_Standard.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForCredit.Append(", " + cogs_StandardAccountName);
                    contraAccountIdsForCredit.Append(", " + AccountHeadConstants.COGS_Standard);
                    var finishedGoodsInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.FinishedGoodsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", " + finishedGoodsInventoryAccountName);
                    contraAccountIdsForDebit.Append(", " + AccountHeadConstants.FinishedGoodsInventory);
                }
                //Transaction entry starts here
                //customer account debit
                await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                 saleInvoice.CustomerId, saleInvoice.NetTotal, 0, saleInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.Sale, transactionQty, transactionQtyValue);
                if (saleInvoice.TotalVat > 0M)
                {
                    //sales account credit
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                        Guid.Parse(AccountHeadConstants.Sales), 0, (saleInvoice.Total - saleInvoice.TotalVat), saleInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                    //Vat on Sales Payable account credit
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                        Guid.Parse(AccountHeadConstants.VatOnSalesPayable), 0, saleInvoice.TotalVat, saleInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

                }
                else
                {
                    //sales account credit
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                        Guid.Parse(AccountHeadConstants.Sales), 0, saleInvoice.Total, saleInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }

                //carrying charge account credit
                if (saleInvoice.TransportationCost != 0m)
                {
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                   Guid.Parse(AccountHeadConstants.CarryingExpenses_Sales), 0, saleInvoice.TransportationCost, saleInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }

                //depot charge account credit
                if (saleInvoice.DepoCharge > 0m)
                {
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.DepotCharge), 0m, saleInvoice.DepoCharge, saleInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }
                //Commission booking
                //monthly discount account debit
                if (totalMonthlyDiscount > 0m)
                {
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.MonthlyDiscount), totalMonthlyDiscount, 0m, saleInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                }
                //yearly discount account debit
                if (totalYearlyDiscount > 0m)
                {
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.YearlyDiscount), totalYearlyDiscount, 0m, saleInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                }
                //target discount account debit
                if (totalTargetDiscount > 0m)
                {
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.TargetDiscount), totalTargetDiscount, 0m, saleInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                }
                //commission payable account credit
                if (totalDiscountBooking > 0m)
                {
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.CommissionPayable), 0m, totalDiscountBooking, saleInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }

                if (string.IsNullOrWhiteSpace(saleInvoice.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(saleInvoice.SaleOrderNo))
                {

                    //cogs account debit
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                         Guid.Parse(AccountHeadConstants.COGS_Standard), cogs_Total, 0, saleInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

                    //Finished Good Inventory account credit
                    await _accountService.HitAccount(_unitOfWork, null, saleInvoice.InvoiceNo, sb.ToString(),
                        Guid.Parse(AccountHeadConstants.FinishedGoodsInventory), 0, cogs_Total, saleInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }


            }
            else throw new BadRequestException("Debit Credit not equal!!!");

        }


        private async Task DeleteTransaction(string saleInvoiceNo)
        {
            var saleInvoice = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.InvoiceNo == saleInvoiceNo);
            if (saleInvoice is null) throw new NotFoundResultException("Sale Invoice Not Found With this invoice No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == saleInvoice.InvoiceNo);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }
        }

        public List<CustomerDiscountViewModel> GetCustomerMonthlyDiscountReport(int year, int? month, Guid? customerId)
        {
            var sales = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(s => s.Status >= (int)SaleInvoiceStatus.Approved && s.InvoiceDate.Year == year);
            if (month.HasValue) sales = sales.Where(x => x.InvoiceDate.Month == month.Value);
            if (customerId.HasValue) sales = sales.Where(x => x.CustomerId == customerId.Value);
            var returns = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.SaleReturn).Where(s => s.SaleReturn.Status == (int)SaleReturnStatus.Approved && s.SaleReturn.SaleReturnDate.Year == year);
            if (month.HasValue) returns = returns.Where(x => x.SaleReturn.SaleReturnDate.Month == month.Value);
            if (customerId.HasValue) returns = returns.Where(x => x.SaleReturn.CustomerId == customerId.Value);
            var customerDiscounts = sales.SelectMany(s => s.SaleInvoiceDetails, (s, i) => new { SaleInvoice = s, SaleInvoiceDetail = i })
                                  .GroupBy(si => new { si.SaleInvoiceDetail.ProductId, si.SaleInvoice.CustomerId })
                                  .Select(g => new CustomerDiscountViewModel
                                  {
                                      CustomerId = g.Key.CustomerId,
                                      ProductId = g.Key.ProductId,
                                      CustomerName = _unitOfWork.Repository<Account>()
                                                        .TableNoTracking().Single(x => x.Id == g.Key.CustomerId).Name,
                                      ProductName = _unitOfWork.Repository<Product>()
                                                        .TableNoTracking().Single(x => x.Id == g.Key.ProductId).Name,
                                      TotalQuantity = g.Sum(si => si.SaleInvoiceDetail.Quantity)
                                      - returns.Where(x => x.SaleReturn.CustomerId == g.Key.CustomerId && x.ProductId == g.Key.ProductId).Sum(x => x.ReturnQuantity),
                                      MonthlyDiscount = g.Sum(si => si.SaleInvoiceDetail.Quantity * si.SaleInvoiceDetail.MonthlyDiscountPerUnit)
                                      - returns.Where(x => x.SaleReturn.CustomerId == g.Key.CustomerId && x.ProductId == g.Key.ProductId).Sum(x => x.ReturnQuantity * x.MonthlyDiscountPerUnit),
                                      YearlyDiscount = g.Sum(si => si.SaleInvoiceDetail.Quantity * si.SaleInvoiceDetail.YearlyDiscountPerUnit)
                                      - returns.Where(x => x.SaleReturn.CustomerId == g.Key.CustomerId && x.ProductId == g.Key.ProductId).Sum(x => x.ReturnQuantity * x.YearlyDiscountPerUnit),
                                      TargetDiscount = g.Sum(si => si.SaleInvoiceDetail.Quantity * si.SaleInvoiceDetail.TargetDiscountPerUnit)
                                      - returns.Where(x => x.SaleReturn.CustomerId == g.Key.CustomerId && x.ProductId == g.Key.ProductId).Sum(x => x.ReturnQuantity * x.TargetDiscountPerUnit),
                                  }).OrderBy(g => g.CustomerName);


            return customerDiscounts.ToList();
        }
        public List<CustomerDiscountViewModel> GetCustomerDateWiseDiscountReport(CustomerDiscountRequestModel request)
        {
            var sales = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(s => s.Status >= (int)SaleInvoiceStatus.Approved && s.InvoiceDate.Date >= request.FromDate.Date && s.InvoiceDate.Date <= request.ToDate.Date);
            if (request.CustomerId.HasValue) sales = sales.Where(x => x.CustomerId == request.CustomerId.Value);
            var returns = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.SaleReturn).Where(s => s.SaleReturn.Status == (int)SaleReturnStatus.Approved && s.SaleReturn.SaleReturnDate.Date >= request.FromDate.Date && s.SaleReturn.SaleReturnDate.Date <= request.ToDate.Date);
            if (request.CustomerId.HasValue) returns = returns.Where(x => x.SaleReturn.CustomerId == request.CustomerId.Value);
            var customerDiscounts = sales.SelectMany(s => s.SaleInvoiceDetails, (s, i) => new { SaleInvoice = s, SaleInvoiceDetail = i })
                                  .GroupBy(si => new { si.SaleInvoiceDetail.ProductId, si.SaleInvoice.CustomerId })
                                  .Select(g => new CustomerDiscountViewModel
                                  {
                                      CustomerId = g.Key.CustomerId,
                                      ProductId = g.Key.ProductId,
                                      CustomerName = _unitOfWork.Repository<Account>()
                                                        .TableNoTracking().Single(x => x.Id == g.Key.CustomerId).Name,
                                      ProductName = _unitOfWork.Repository<Product>()
                                                        .TableNoTracking().Single(x => x.Id == g.Key.ProductId).Name,
                                      TotalQuantity = g.Sum(si => si.SaleInvoiceDetail.Quantity)
                                      - returns.Where(x => x.SaleReturn.CustomerId == g.Key.CustomerId && x.ProductId == g.Key.ProductId).Sum(x => x.ReturnQuantity),
                                      MonthlyDiscount = g.Sum(si => si.SaleInvoiceDetail.Quantity * si.SaleInvoiceDetail.MonthlyDiscountPerUnit)
                                      - returns.Where(x => x.SaleReturn.CustomerId == g.Key.CustomerId && x.ProductId == g.Key.ProductId).Sum(x => x.ReturnQuantity * x.MonthlyDiscountPerUnit),
                                      YearlyDiscount = g.Sum(si => si.SaleInvoiceDetail.Quantity * si.SaleInvoiceDetail.YearlyDiscountPerUnit)
                                      - returns.Where(x => x.SaleReturn.CustomerId == g.Key.CustomerId && x.ProductId == g.Key.ProductId).Sum(x => x.ReturnQuantity * x.YearlyDiscountPerUnit),
                                      TargetDiscount = g.Sum(si => si.SaleInvoiceDetail.Quantity * si.SaleInvoiceDetail.TargetDiscountPerUnit)
                                      - returns.Where(x => x.SaleReturn.CustomerId == g.Key.CustomerId && x.ProductId == g.Key.ProductId).Sum(x => x.ReturnQuantity * x.TargetDiscountPerUnit),
                                  }).OrderBy(g => g.CustomerName);


            return customerDiscounts.ToList();
        }

        public async Task<List<SalesItemViewModel>> CustomerWiseSalesItemSearchAsync(SaleInvoiceRequestModel request)
        {
            var queryable = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking()
                .Include(x => x.SaleInvoice).ThenInclude(x => x.Customer)
                .Include(x => x.SaleInvoice).ThenInclude(x => x.Store).AsQueryable();
            queryable = queryable.Where(x => x.SaleInvoice.Status >= (int)SaleInvoiceStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue) queryable = queryable.Where(d => d.SaleInvoice.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleInvoice.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);
            if (request.CustomerId.HasValue) queryable = queryable.Where(d => d.SaleInvoice.CustomerId == request.CustomerId.Value);
            if (request.ProductId.HasValue) queryable = queryable.Where(d => d.ProductId == request.ProductId.Value);
            if (request.StoreId.HasValue) queryable = queryable.Where(d => d.SaleInvoice.StoreId == request.StoreId.Value);
            if (request.CustomerZoneId.HasValue) queryable = queryable.Where(x => x.SaleInvoice.Customer.CustomerZoneId == request.CustomerZoneId.Value);
            if (request.CustomerAreaId.HasValue) queryable = queryable.Where(x => x.SaleInvoice.Customer.CustomerAreaId == request.CustomerAreaId.Value);
            if (request.CustomerMarketingOfficerId.HasValue) queryable = queryable.Where(x => x.SaleInvoice.Customer.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId.Value);
            var salesInvoiceItems = await queryable.Select(d => new SalesItemViewModel
            {
                Id = d.Id,
                BillNo = d.SaleInvoice.InvoiceNo,
                Date = d.SaleInvoice.InvoiceDate,
                CustomerName = d.SaleInvoice.Customer.Name,
                StoreName = d.SaleInvoice.Store.Name,
                ProductTypeName = d.Product.ProductType.Name,
                BagWeight = d.Product.BagWeight,
                PrimaryQuantity = d.PrimaryQuantity,
                ProductName = d.Product.Name,
                Description = d.SaleInvoice.Remark,
                SaleOrderNo = d.SaleInvoice.SaleOrderNo,
                Quantity = d.Quantity,
                InvoiceDiscountPerUnit = d.InvoiceDiscountPerUnit,
                CashDiscountPerUnit = d.CashDiscountPerUnit,
                SpecialDiscountPerUnit = d.SpecialDiscountPerUnit,
                OfferDiscountPerUnit = d.OfferDiscountPerUnit,
                OtherDiscountPerUnit = d.OtherDiscountPerUnit,
                MonthlyDiscountPerUnit = d.MonthlyDiscountPerUnit,
                YearlyDiscountPerUnit = d.YearlyDiscountPerUnit,
                TargetDiscountPerUnit = d.TargetDiscountPerUnit,
                Rate = d.Rate,
                // IMPORTANT:
                // In DB DiscountPerUnit = InvoiceDiscountPerUnit + CashDiscountPerUnit + SpecialDiscountPerUnit
                // d.NetRate (DB) = Rate - DiscountPerUnit - OfferDiscountPerUnit
                // 'OtherDiscountPerUnit' is calculated separately and NOT included in d.NetRate
                // For PDF Report, to display final 'NetRate' need to subtract 'OtherDiscountPerUnit'
                InvoiceNetRate = d.NetRate - d.OtherDiscountPerUnit,
                NetRate = d.NetRate - d.OtherDiscountPerUnit - d.MonthlyDiscountPerUnit - d.YearlyDiscountPerUnit - d.TargetDiscountPerUnit,
                Value = d.Quantity * ( d.NetRate - d.OtherDiscountPerUnit),
                NetAmount = d.Quantity * (d.NetRate - d.OtherDiscountPerUnit - d.MonthlyDiscountPerUnit - d.YearlyDiscountPerUnit - d.TargetDiscountPerUnit),
            }).OrderBy(x => x.Date).ToListAsync();

            return salesInvoiceItems;
        }

        public async Task<SalesReportViewModel> SalesReportSearchAsync(SaleInvoiceRequestModel request)
        {
            var salesItem = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.SaleInvoice).Where(x => x.SaleInvoice.Status >= (int)SaleInvoiceStatus.Approved).AsQueryable();
            if (request.FromDate.HasValue && request.ToDate.HasValue) salesItem = salesItem.Where(d => d.SaleInvoice.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleInvoice.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);

            var saleInvoice = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(x => x.Status >= (int)SaleInvoiceStatus.Approved).AsQueryable();
            if (request.FromDate.HasValue && request.ToDate.HasValue) saleInvoice = saleInvoice.Where(d => d.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);

            var salesReturn = _unitOfWork.Repository<SaleReturn>().TableNoTracking().Where(x => x.Status >= (int)SaleReturnStatus.Approved).AsQueryable();
            if (request.FromDate.HasValue && request.ToDate.HasValue) salesReturn = salesReturn.Where(d => d.SaleReturnDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleReturnDate.Date <= request.ToDate.Value.ToLocal().Date);

            var salesReturnItem = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.SaleReturn).Where(x => x.SaleReturn.Status >= (int)SaleReturnStatus.Approved).AsQueryable();
            if (request.FromDate.HasValue && request.ToDate.HasValue) salesReturnItem = salesReturnItem.Where(d => d.SaleReturn.SaleReturnDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleReturn.SaleReturnDate.Date <= request.ToDate.Value.ToLocal().Date);


            var result = new SalesReportViewModel()
            {
                TotalSales = await saleInvoice.SumAsync(x => x.Subtotal),
                TotalSalesReturn = await salesReturn.SumAsync(x => x.Total),
                TotalOfferDiscount = await saleInvoice.SumAsync(x => x.OfferDiscount),
                TotalOtherDiscount = await saleInvoice.SumAsync(x => x.OtherDiscount),
                TotalInvoiceDiscount = await salesItem.SumAsync(x => x.InvoiceDiscountPerUnit * x.Quantity),
                TotalCashDiscount = await salesItem.SumAsync(x => x.CashDiscountPerUnit * x.Quantity),
                TotalSpecialDiscount = await salesItem.SumAsync(x => x.SpecialDiscountPerUnit * x.Quantity),
                TotalMonthlyDiscount = await salesItem.SumAsync(x => x.MonthlyDiscountPerUnit * x.Quantity) -
                await salesReturnItem.SumAsync(x => x.MonthlyDiscountPerUnit * x.ReturnQuantity),
                TotalYearlyDiscount = await salesItem.SumAsync(x => x.YearlyDiscountPerUnit * x.Quantity) -
                 await salesReturnItem.SumAsync(x => x.YearlyDiscountPerUnit * x.ReturnQuantity),
                TotalTargetDiscount = await salesItem.SumAsync(x => x.TargetDiscountPerUnit * x.Quantity) -
                 await salesReturnItem.SumAsync(x => x.TargetDiscountPerUnit * x.ReturnQuantity),
            };
            return result;
        }

        public async Task<List<SalesTotalMonthWiseViewModel>> SalesTotalMonthWiseSearchAsync(SaleInvoiceRequestModel request)
        {
            var saleInvoice = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().AsQueryable();
            saleInvoice = saleInvoice.Where(x => x.Status >= (int)SaleInvoiceStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue) saleInvoice = saleInvoice.Where(d => d.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);

            var saleInvoiceDetail = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.SaleInvoice).AsQueryable();
            saleInvoiceDetail = saleInvoiceDetail.Where(x => x.SaleInvoice.Status >= (int)SaleInvoiceStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue) saleInvoiceDetail = saleInvoiceDetail.Where(d => d.SaleInvoice.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleInvoice.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);

            var saleReturnDetail = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.SaleReturn).AsQueryable();
            saleReturnDetail = saleReturnDetail.Where(x => x.SaleReturn.Status >= (int)SaleReturnStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue) saleReturnDetail = saleReturnDetail.Where(d => d.SaleReturn.SaleReturnDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleReturn.SaleReturnDate.Date <= request.ToDate.Value.ToLocal().Date);

            var saleInvoiceGrouped = await saleInvoice.GroupBy(x => new { x.InvoiceDate.Year, x.InvoiceDate.Month })
                .Select(d => new SalesTotalMonthWiseViewModel
                {
                    Year = d.Key.Year,
                    Month = d.Key.Month,
                    SaleQuantity = 0,
                    SaleAmount = 0,
                    SaleCommission = d.Sum(x => x.OtherDiscount),
                    TransportationCost = d.Sum(x => x.TransportationCost),
                    DepoCharge = d.Sum(x => x.DepoCharge),
                    SaleReturnValue = 0,
                    SaleReturnQuantity = 0,
                }).ToListAsync();

            var saleInvoiceDetailGrouped = await saleInvoiceDetail.GroupBy(x => new { Year = x.SaleInvoice.InvoiceDate.Year, Month = x.SaleInvoice.InvoiceDate.Month })
                .Select(d => new SalesTotalMonthWiseViewModel
                {
                    Year = d.Key.Year,
                    Month = d.Key.Month,
                    SaleQuantity = d.Sum(x => x.Quantity),
                    SaleAmount = d.Sum(x => x.Amount),
                    SaleCommission = d.Sum(x => (x.InvoiceDiscountPerUnit * x.Quantity) + (x.CashDiscountPerUnit * x.Quantity) + (x.SpecialDiscountPerUnit * x.Quantity) + (x.OfferDiscountPerUnit * x.Quantity)),
                    TransportationCost = 0,
                    DepoCharge = 0,
                    SaleReturnValue = 0,
                    SaleReturnQuantity = 0,
                }).ToListAsync();

            var combinedSaleInvoiceData = saleInvoiceGrouped.Union(saleInvoiceDetailGrouped).GroupBy(x => new { x.Year, x.Month })
                .Select(group => new SalesTotalMonthWiseViewModel
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    SaleQuantity = group.Sum(x => x.SaleQuantity),
                    SaleAmount = group.Sum(x => x.SaleAmount),
                    SaleCommission = group.Sum(x => x.SaleCommission),
                    TransportationCost = group.Sum(x => x.TransportationCost),
                    DepoCharge = group.Sum(x => x.DepoCharge),
                    SaleReturnQuantity = group.Sum(x => x.SaleReturnQuantity),
                    SaleReturnValue = group.Sum(x => x.SaleReturnValue)
                }).OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();

            var saleReturnDetailGrouped = await saleReturnDetail.GroupBy(x => new { Year = x.SaleReturn.SaleReturnDate.Year, Month = x.SaleReturn.SaleReturnDate.Month })
                .Select(d => new SalesTotalMonthWiseViewModel
                {
                    Year = d.Key.Year,
                    Month = d.Key.Month,
                    SaleQuantity = 0,
                    SaleAmount = 0,
                    SaleCommission = 0,
                    TransportationCost = 0,
                    DepoCharge = 0,
                    SaleReturnValue = d.Sum(x => x.Amount),
                    SaleReturnQuantity = d.Sum(x => x.ReturnQuantity)
                }).ToListAsync();

            var salesTotalMonthWise = combinedSaleInvoiceData.Union(saleReturnDetailGrouped).GroupBy(x => new { x.Year, x.Month })
                .Select(group => new SalesTotalMonthWiseViewModel()
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    SaleQuantity = group.Sum(x => x.SaleQuantity),
                    SaleAmount = group.Sum(x => x.SaleAmount),
                    SaleCommission = group.Sum(x => x.SaleCommission),
                    TransportationCost = group.Sum(x => x.TransportationCost),
                    DepoCharge = group.Sum(x => x.DepoCharge),
                    SaleReturnQuantity = group.Sum(x => x.SaleReturnQuantity),
                    SaleReturnValue = group.Sum(x => x.SaleReturnValue)

                }).OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
            return salesTotalMonthWise;
        }
        public async Task<List<CustomerLedgerFeedWiseViewModel>> CustomerLedgerFeedWiseSearchAsync(CustomerLedgerFeedWiseRequestModel request)
        {
            var saleInvoiceDetail = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.SaleInvoice).Where(x => x.SaleInvoice.CustomerId == request.AccountId && x.SaleInvoice.Status >= (int)SaleInvoiceStatus.Approved).AsQueryable();
            var saleReturnDetail = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.SaleReturn).Where(x => x.SaleReturn.CustomerId == request.AccountId && x.SaleReturn.Status >= (int)SaleReturnStatus.Approved).AsQueryable();

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(d => d.SaleInvoice.InvoiceDate.Date >= request.FromDate.Value.Date && d.SaleInvoice.InvoiceDate.Date <= request.ToDate.Value.Date);
                saleReturnDetail = saleReturnDetail.Where(d => d.SaleReturn.SaleReturnDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleReturn.SaleReturnDate.Date <= request.ToDate.Value.ToLocal().Date);
            }

            var saleInvoiceDetailGrouped = await saleInvoiceDetail.Select(d => new CustomerLedgerFeedWiseViewModel
            {
                Id = d.Id,
                InvoiceNo = d.SaleInvoice.InvoiceNo,
                DeliveryNoteNo = d.SaleInvoice.DeliveryNoteNo,
                Date = d.SaleInvoice.InvoiceDate,
                ProductTypeName = d.Product.ProductType.Name,
                ProductName = d.Product.Name,
                Quantity = d.Quantity,
                Rate = d.Rate,
                CommissionRate = d.DiscountPerUnit,
                OfferDiscountPerUnit = d.OfferDiscountPerUnit,
                OtherDiscountPerUnit = d.OtherDiscountPerUnit,

                // IMPORTANT:
                // DiscountPerUnit = InvoiceDiscountPerUnit + CashDiscountPerUnit + SpecialDiscountPerUnit
                // d.NetRate (DB) = Rate - DiscountPerUnit - OfferDiscountPerUnit
                // 'OtherDiscountPerUnit' is calculated separately and NOT included in d.NetRate
                // For PDF Report, to display final 'NetRate' need to subtract 'OtherDiscountPerUnit'
                NetRate = d.NetRate - d.OtherDiscountPerUnit,
                RateAmount = d.Quantity * d.Rate,
                CommissionAmount = d.Quantity * d.DiscountPerUnit,
                OfferDiscountAmount = d.Quantity * d.OfferDiscountPerUnit,
                OtherDiscountAmount = d.Quantity * d.OtherDiscountPerUnit,

                // IMPORTANT:
                // NetRateAmount = (Quantity × DB Stored NetRate) - (Quantity × OtherDiscountPerUnit)
                // Beacuse OtherDiscountPerUnit is not included in DB NetRate
                NetRateAmount = (d.Quantity * d.NetRate) - (d.Quantity * d.OtherDiscountPerUnit),
                DepoChargePerKg = d.SaleInvoice.DepoCharge / d.SaleInvoice.SaleInvoiceDetails.Sum(x => x.Quantity),
            }).OrderBy(x => x.Date).ToListAsync();

            var saleReturnDetailGrouped = await saleReturnDetail.Select(d => new CustomerLedgerFeedWiseViewModel
            {
                Id = d.Id,
                InvoiceNo = d.SaleReturn.SaleReturnNo,
                DeliveryNoteNo = d.SaleReturn.DeliveryNoteNo,
                Date = d.SaleReturn.SaleReturnDate,
                ProductTypeName = d.Product.ProductType.Name,
                ProductName = d.Product.Name,
                Quantity = -d.ReturnQuantity,
                Rate = d.Rate,
                CommissionRate = 0m,
                NetRate = d.Rate,
                RateAmount = -d.Amount,
                CommissionAmount = 0m,
                NetRateAmount = -d.Amount,
                DepoChargePerKg = 0m,
            }).OrderBy(x => x.Date).ToListAsync();

            var customerLedgerFeedWise = saleInvoiceDetailGrouped.Union(saleReturnDetailGrouped).Select(group => new CustomerLedgerFeedWiseViewModel()
            {
                Id = group.Id,
                InvoiceNo = group.InvoiceNo,
                DeliveryNoteNo = group.DeliveryNoteNo,
                Date = group.Date,
                ProductTypeName = group.ProductTypeName,
                ProductName = group.ProductName,
                Quantity = group.Quantity,
                Rate = group.Rate,
                CommissionRate = group.CommissionRate,
                OfferDiscountPerUnit = group.OfferDiscountPerUnit,
                OtherDiscountPerUnit = group.OtherDiscountPerUnit,
                NetRate = group.NetRate,
                RateAmount = group.RateAmount,
                CommissionAmount = group.CommissionAmount,
                OfferDiscountAmount = group.OfferDiscountAmount,
                OtherDiscountAmount = group.OtherDiscountAmount,
                NetRateAmount = group.NetRateAmount,
                DepoChargePerKg = group.DepoChargePerKg,
            }).OrderBy(x => x.Date).ToList();
            return customerLedgerFeedWise;
        }

        public async Task<List<SalesTotalDateWiseViewModel>> SalesTotalDateWiseSearchAsync(SaleInvoiceRequestModel request)
        {
            var saleInvoice = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().AsQueryable();
            saleInvoice = saleInvoice.Where(x => x.Status >= (int)SaleInvoiceStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue) saleInvoice = saleInvoice.Where(d => d.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);

            var saleInvoiceDetail = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.SaleInvoice).AsQueryable();
            saleInvoiceDetail = saleInvoiceDetail.Where(x => x.SaleInvoice.Status >= (int)SaleInvoiceStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue) saleInvoiceDetail = saleInvoiceDetail.Where(d => d.SaleInvoice.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleInvoice.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);

            var saleReturnDetail = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.SaleReturn).AsQueryable();
            saleReturnDetail = saleReturnDetail.Where(x => x.SaleReturn.Status >= (int)SaleReturnStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue) saleReturnDetail = saleReturnDetail.Where(d => d.SaleReturn.SaleReturnDate.Date >= request.FromDate.Value.ToLocal().Date && d.SaleReturn.SaleReturnDate.Date <= request.ToDate.Value.ToLocal().Date);

            var receivePayment = _unitOfWork.Repository<ReceivePayment>().TableNoTracking().AsQueryable();
            receivePayment = receivePayment.Where(x => x.Status >= (int)ReceivePaymentStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue) receivePayment = receivePayment.Where(d => d.PaymentDate.Date >= request.FromDate.Value.ToLocal().Date && d.PaymentDate.Date <= request.ToDate.Value.ToLocal().Date);

            var saleInvoiceGrouped = await saleInvoice.GroupBy(x => x.InvoiceDate)
                .Select(d => new SalesTotalDateWiseViewModel
                {
                    Date = d.Key.Date,
                    SaleQuantity = 0,
                    SaleAmount = 0,
                    SaleCommission = d.Sum(x => x.OtherDiscount),
                    TransportationCost = d.Sum(x => x.TransportationCost),
                    DepoCharge = d.Sum(x => x.DepoCharge),
                    SaleReturnValue = 0,
                    SaleReturnQuantity = 0,
                }).ToListAsync();

            var saleInvoiceDetailGrouped = await saleInvoiceDetail.GroupBy(x => x.SaleInvoice.InvoiceDate)
                .Select(d => new SalesTotalDateWiseViewModel
                {
                    Date = d.Key.Date,
                    SaleQuantity = d.Sum(x => x.Quantity),
                    SaleAmount = d.Sum(x => x.Amount),
                    SaleCommission = d.Sum(x => (x.InvoiceDiscountPerUnit * x.Quantity) + (x.CashDiscountPerUnit * x.Quantity) + (x.SpecialDiscountPerUnit * x.Quantity) + (x.OfferDiscountPerUnit * x.Quantity)),
                    TransportationCost = 0,
                    DepoCharge = 0,
                    SaleReturnValue = 0,
                    SaleReturnQuantity = 0,
                }).ToListAsync();

            var combinedSaleInvoiceData = saleInvoiceGrouped.Union(saleInvoiceDetailGrouped).GroupBy(x => x.Date)
                .Select(group => new SalesTotalDateWiseViewModel
                {
                    Date = group.Key.Date,
                    SaleQuantity = group.Sum(x => x.SaleQuantity),
                    SaleAmount = group.Sum(x => x.SaleAmount),
                    SaleCommission = group.Sum(x => x.SaleCommission),
                    TransportationCost = group.Sum(x => x.TransportationCost),
                    DepoCharge = group.Sum(x => x.DepoCharge),
                    SaleReturnQuantity = group.Sum(x => x.SaleReturnQuantity),
                    SaleReturnValue = group.Sum(x => x.SaleReturnValue)
                }).OrderBy(x => x.Date).ToList();

            var saleReturnDetailGrouped = await saleReturnDetail.GroupBy(x => x.SaleReturn.SaleReturnDate)
                .Select(d => new SalesTotalDateWiseViewModel
                {
                    Date = d.Key.Date,
                    SaleQuantity = 0,
                    SaleAmount = 0,
                    SaleCommission = 0,
                    TransportationCost = 0,
                    DepoCharge = 0,
                    SaleReturnValue = d.Sum(x => x.Amount),
                    SaleReturnQuantity = d.Sum(x => x.ReturnQuantity)
                }).ToListAsync();

            var salesTotalMonthWise = combinedSaleInvoiceData.Union(saleReturnDetailGrouped).GroupBy(x => x.Date)
                .Select(group => new SalesTotalDateWiseViewModel()
                {
                    Date = group.Key.Date,
                    SaleQuantity = group.Sum(x => x.SaleQuantity),
                    SaleAmount = group.Sum(x => x.SaleAmount),
                    SaleCommission = group.Sum(x => x.SaleCommission),
                    TransportationCost = group.Sum(x => x.TransportationCost),
                    DepoCharge = group.Sum(x => x.DepoCharge),
                    SaleReturnQuantity = group.Sum(x => x.SaleReturnQuantity),
                    SaleReturnValue = group.Sum(x => x.SaleReturnValue)
                }).OrderBy(x => x.Date).ToList();
            return salesTotalMonthWise;
        }
        public async Task<List<ExpandoObject>> MonthWiseProductSales()
        {
            List<ExpandoObject> finalResult = new List<ExpandoObject>();
            var queryable = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.Product).Include(x => x.SaleInvoice).AsQueryable();
            queryable = queryable.Where(x => x.SaleInvoice.Status >= (int)SaleInvoiceStatus.Approved);

            var startDate = new DateTime(DateTime.Now.Year - 1, 7, 1); // Start of the financial year (July 1st)
            var endDate = new DateTime(DateTime.Now.Year, 6, 30); // End of the financial year (June 30th of the next year)
            var salesData = await queryable
                 .Where(x => startDate <= x.SaleInvoice.InvoiceDate.Date && endDate >= x.SaleInvoice.InvoiceDate.Date)
                 .GroupBy(s => new { s.SaleInvoice.InvoiceDate.Year, s.SaleInvoice.InvoiceDate.Month, s.Product.Name })
                 .Select(group => new
                 {
                     Year = group.Key.Year,
                     Month = group.Key.Month,
                     MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Key.Month),
                     ProductName = group.Key.Name,
                     TotalSales = group.Sum(s => s.Quantity) - group.Sum(s => s.ReturnQuantity)
                 }).OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync();

            var uniqueMonthNames = salesData.GroupBy(item => item.MonthName).Select(group => group.Key).ToList();
            var pivotData = salesData.GroupBy(x => x.ProductName).Select(group => new
            {
                ProductName = group.Key,
                MonthSummaries = group.ToDictionary(item => item.MonthName, item => item.TotalSales / 1000m)//in metric ton
            }).OrderBy(x => x.ProductName).ToList();

            Dictionary<string, Dictionary<string, decimal>> salesMonthWise = new Dictionary<string, Dictionary<string, decimal>>();
            foreach (var pivot in pivotData)
            {
                Dictionary<string, decimal> produced = new Dictionary<string, decimal>();
                foreach (var name in uniqueMonthNames)
                {
                    if (pivot.MonthSummaries.ContainsKey(name)) produced[name] = pivot.MonthSummaries[name];
                    else produced[name] = 0;
                }
                salesMonthWise.Add(pivot.ProductName, produced);
            }
            foreach (var data in salesMonthWise)
            {
                ExpandoObject obj = CreateDynamicObjectFromDictionary(data.Key, data.Value);
                finalResult.Add(obj);
            }
            return finalResult;
        }
        private ExpandoObject CreateDynamicObjectFromDictionary(string key, Dictionary<string, decimal> dictionary)
        {
            dynamic customObject = new ExpandoObject();
            var customObjectDictionary = (IDictionary<string, object>)customObject;
            customObjectDictionary["Product"] = key;
            decimal summation = 0m;
            foreach (var kvp in dictionary)
            {
                customObjectDictionary[kvp.Key] = kvp.Value;
                summation += kvp.Value;
            }
            customObjectDictionary["Total"] = summation;
            return customObject;
        }

        public async Task<List<SalesItemDetailViewModel>> SalesItemSearchAsync(SalesItemDetailRequestModel request)
        {
            var saleInvoiceDetail = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking()
                .Include(x => x.SaleInvoice).ThenInclude(x => x.Customer)
                .Include(x => x.SaleInvoice).ThenInclude(x => x.Store).Include(x => x.SaleInvoice).ThenInclude(x => x.CustomerTerritory).AsQueryable();
            saleInvoiceDetail = saleInvoiceDetail.Where(x => x.SaleInvoice.Status >= (int)SaleInvoiceStatus.Approved);

            var saleReturnDetail = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.SaleReturn).ThenInclude(x => x.Customer).Include(x => x.SaleReturn).ThenInclude(x => x.Store).AsQueryable();
            saleReturnDetail = saleReturnDetail.Where(x => x.SaleReturn.Status >= (int)SaleReturnStatus.Approved);

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(d => d.SaleInvoice.InvoiceDate.Date >= request.FromDate.Value.Date && d.SaleInvoice.InvoiceDate.Date <= request.ToDate.Value.Date);
                saleReturnDetail = saleReturnDetail.Where(d => d.SaleReturn.SaleReturnDate.Date >= request.FromDate.Value.Date && d.SaleReturn.SaleReturnDate.Date <= request.ToDate.Value.Date);
            }
            if (request.CustomerId.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(d => d.SaleInvoice.CustomerId == request.CustomerId.Value);
                saleReturnDetail = saleReturnDetail.Where(d => d.SaleReturn.CustomerId == request.CustomerId.Value);
            }
            if (request.StoreId.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(d => d.SaleInvoice.StoreId == request.StoreId.Value);
                saleReturnDetail = saleReturnDetail.Where(d => d.SaleReturn.StoreId == request.StoreId.Value);
            }
            if (request.ProductId.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(d => d.ProductId == request.ProductId.Value);
                saleReturnDetail = saleReturnDetail.Where(d => d.ProductId == request.ProductId.Value);
            }
            if (request.CustomerRegionId.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(x => x.SaleInvoice.Customer.CustomerRegionId == request.CustomerRegionId.Value);
                saleReturnDetail = saleReturnDetail.Where(x => x.SaleReturn.Customer.CustomerRegionId == request.CustomerRegionId.Value);
            }
            if (request.CustomerZoneId.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(x => x.SaleInvoice.Customer.CustomerZoneId == request.CustomerZoneId.Value);
                saleReturnDetail = saleReturnDetail.Where(x => x.SaleReturn.Customer.CustomerZoneId == request.CustomerZoneId.Value);
            }
            if (request.CustomerAreaId.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(x => x.SaleInvoice.Customer.CustomerAreaId == request.CustomerAreaId.Value);
                saleReturnDetail = saleReturnDetail.Where(x => x.SaleReturn.Customer.CustomerAreaId == request.CustomerAreaId.Value);
            }
            if (request.CustomerTerritoryId.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(x => x.SaleInvoice.CustomerTerritoryId == request.CustomerTerritoryId.Value);
                saleReturnDetail = saleReturnDetail.Where(x => x.SaleReturn.CustomerTerritoryId == request.CustomerTerritoryId.Value);
            }
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                saleInvoiceDetail = saleInvoiceDetail.Where(x => x.SaleInvoice.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId.Value);
                saleReturnDetail = saleReturnDetail.Where(x => x.SaleReturn.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId.Value);
            }
            var salesInvoiceItems = await saleInvoiceDetail.Select(d => new SalesItemDetailViewModel
            {
                RegionId = d.SaleInvoice.Customer.CustomerRegionId,
                ZoneId = d.SaleInvoice.Customer.CustomerZoneId,
                AreaId = d.SaleInvoice.Customer.CustomerAreaId,
                TerritoryId = d.SaleInvoice.CustomerTerritoryId,
                MarketingOfficerId = d.SaleInvoice.CustomerMarketingOfficerId,
                CustomerId = d.SaleInvoice.CustomerId,
                CustomerName = d.SaleInvoice.Customer.Name,
                CustomerAddress = d.SaleInvoice.Customer.Address,
                SaleInvoiceNo = d.SaleInvoice!.InvoiceNo,
                ProductId = d.ProductId,
                ProductCode = d.Product.Code,
                ProductName = d.Product.Name,
                PackSize = d.Product.PackSize.Name,
                ProductTypeName = d.Product.ProductType.Name,
                DispatchQuantity = d.Quantity,
                TradePrice = d.Rate,
                DispatchValue = d.Quantity * d.Rate,
                InvoiceDiscountAmount = d.PercentageDiscountAmount,
                BonusQuantity = d.BonusQuantity,
                ReturnBonusQuantity = 0,
                ReturnDispatchQuantity = 0,
                ReturnValue = 0M,
                ReturnDiscountAmount = 0M,
                TotalVat = (d.Quantity * d.Rate * d.VatPercentage) / (100M + d.VatPercentage)
            }).OrderBy(x => x.ProductName).ToListAsync();

            var salesReturnItems = await saleReturnDetail.Select(d => new SalesItemDetailViewModel
            {
                RegionId = d.SaleReturn.Customer.CustomerRegionId,
                ZoneId = d.SaleReturn.Customer.CustomerZoneId,
                AreaId = d.SaleReturn.Customer.CustomerAreaId,
                TerritoryId = d.SaleReturn.CustomerTerritoryId,
                MarketingOfficerId = d.SaleReturn.CustomerMarketingOfficerId,
                CustomerId = d.SaleReturn.CustomerId,
                CustomerName = d.SaleReturn.Customer.Name,
                CustomerAddress = d.SaleReturn.Customer.Address,
                SaleInvoiceNo = d.SaleReturn!.InvoiceNo,
                ProductId = d.ProductId,
                ProductCode = d.Product.Code,
                ProductName = d.Product.Name,
                PackSize = d.Product.PackSize.Name,
                ProductTypeName = d.Product.ProductType.Name,
                DispatchQuantity = 0,
                TradePrice = 0M,
                DispatchValue = 0M,
                InvoiceDiscountAmount = 0M,
                BonusQuantity = 0,
                ReturnBonusQuantity = d.ReturnBonusQuantity,
                ReturnDispatchQuantity = d.ReturnQuantity,
                ReturnValue = d.Amount,
                ReturnDiscountAmount = d.PercentageDiscountAmount,
                TotalVat = (d.ReturnQuantity * d.Rate * d.VatPercentage) / (100M + d.VatPercentage)
            }).OrderBy(x => x.ProductName).ToListAsync();

            // Combine the lists
            var combinedSalesItems = salesInvoiceItems.Concat(salesReturnItems)
                                                      .OrderBy(x => x.ProductName)
                                                      .ToList();

            return combinedSalesItems;
        }

        public async Task<List<SalesAgingReportViewModel>> SalesAgingDataSearchAsync(SalesItemDetailRequestModel request)
        {
            var saleInvoice = _unitOfWork.Repository<SaleInvoice>().TableNoTracking()
                .Include(x => x.Customer).Include(x => x.Store).Include(x => x.CustomerTerritory).AsQueryable();
            saleInvoice = saleInvoice.Where(x => x.Status >= (int)SaleInvoiceStatus.Approved);

            var saleReturn = _unitOfWork.Repository<SaleReturn>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).AsQueryable();
            saleReturn = saleReturn.Where(x => x.Status >= (int)SaleReturnStatus.Approved);

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                saleInvoice = saleInvoice.Where(d => d.InvoiceDate.Date >= request.FromDate.Value.Date && d.InvoiceDate.Date <= request.ToDate.Value.Date);
                saleReturn = saleReturn.Where(d => d.SaleReturnDate.Date >= request.FromDate.Value.Date && d.SaleReturnDate.Date <= request.ToDate.Value.Date);
            }
            if (request.CustomerId.HasValue)
            {
                saleInvoice = saleInvoice.Where(d => d.CustomerId == request.CustomerId.Value);
                saleReturn = saleReturn.Where(d => d.CustomerId == request.CustomerId.Value);
            }
            if (request.StoreId.HasValue)
            {
                saleInvoice = saleInvoice.Where(d => d.StoreId == request.StoreId.Value);
                saleReturn = saleReturn.Where(d => d.StoreId == request.StoreId.Value);
            }
            if (request.CustomerRegionId.HasValue)
            {
                saleInvoice = saleInvoice.Where(x => x.Customer.CustomerRegionId == request.CustomerRegionId.Value);
                saleReturn = saleReturn.Where(x => x.Customer.CustomerRegionId == request.CustomerRegionId.Value);
            }
            if (request.CustomerZoneId.HasValue)
            {
                saleInvoice = saleInvoice.Where(x => x.Customer.CustomerZoneId == request.CustomerZoneId.Value);
                saleReturn = saleReturn.Where(x => x.Customer.CustomerZoneId == request.CustomerZoneId.Value);
            }
            if (request.CustomerAreaId.HasValue)
            {
                saleInvoice = saleInvoice.Where(x => x.Customer.CustomerAreaId == request.CustomerAreaId.Value);
                saleReturn = saleReturn.Where(x => x.Customer.CustomerAreaId == request.CustomerAreaId.Value);
            }
            if (request.CustomerTerritoryId.HasValue)
            {
                saleInvoice = saleInvoice.Where(x => x.CustomerTerritoryId == request.CustomerTerritoryId.Value);
                saleReturn = saleReturn.Where(x => x.CustomerTerritoryId == request.CustomerTerritoryId.Value);
            }
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                saleInvoice = saleInvoice.Where(x => x.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId.Value);
                saleReturn = saleReturn.Where(x => x.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId.Value);
            }
            var salesInvoiceSummary = await saleInvoice.Select(d => new SalesAgingReportViewModel
            {
                RegionId = d.Customer.CustomerRegionId,
                ZoneId = d.Customer.CustomerZoneId,
                AreaId = d.Customer.CustomerAreaId,
                TerritoryId = d.CustomerTerritoryId,
                MarketingOfficerId = d.CustomerMarketingOfficerId,
                CustomerId = d.CustomerId,
                CustomerName = d.Customer.Name,
                SaleInvoiceNo = d.InvoiceNo,
                InvoiceDate = d.InvoiceDate,
                OverDueDays = (DateTime.Now - d.InvoiceDate).Days,
                PaymentTerm = d.PaymentTerm,
                DispatchValue = d.NetTotal,
                Paid = d.Paid,
                AvailableReceivable = d.AvailableReceivable,
                InvoiceDiscountAmount = d.TotalPercentageDiscountAmount,
                ReturnValue = 0M,
                ReturnDiscountAmount = 0M
            }).OrderBy(x => x.SaleInvoiceNo).ToListAsync();

            // Fetch Sales Return Data (Only Matching Invoices)
            var salesReturnSummary = await saleReturn
                .Where(d => salesInvoiceSummary.Select(si => si.SaleInvoiceNo).Contains(d.InvoiceNo))
                .Select(d => new SalesAgingReportViewModel
                {
                    RegionId = d.Customer.CustomerRegionId,
                    ZoneId = d.Customer.CustomerZoneId,
                    AreaId = d.Customer.CustomerAreaId,
                    TerritoryId = d.CustomerTerritoryId,
                    MarketingOfficerId = d.CustomerMarketingOfficerId,
                    CustomerId = d.CustomerId,
                    CustomerName = d.Customer.Name,
                    SaleInvoiceNo = d.InvoiceNo,
                    InvoiceDate = null,
                    OverDueDays = 0,
                    PaymentTerm = 0,
                    DispatchValue = 0M,
                    Paid = 0M,
                    AvailableReceivable = 0M,
                    InvoiceDiscountAmount = 0M,
                    ReturnValue = d.Total,
                    ReturnDiscountAmount = d.TotalPercentageDiscountAmount
                }).ToListAsync();

            var mergedSummary = salesInvoiceSummary
                .Union(salesReturnSummary)
                .GroupBy(d => d.SaleInvoiceNo)
                .Select(g => new SalesAgingReportViewModel
                {
                    RegionId = g.First().RegionId,
                    ZoneId = g.First().ZoneId,
                    AreaId = g.First().AreaId,
                    TerritoryId = g.First().TerritoryId,
                    MarketingOfficerId = g.First().MarketingOfficerId,
                    CustomerId = g.First().CustomerId,
                    CustomerName = g.First().CustomerName,
                    SaleInvoiceNo = g.Key,
                    InvoiceDate = g.First().InvoiceDate,
                    OverDueDays = g.First().OverDueDays,
                    PaymentTerm = g.First().PaymentTerm,
                    DispatchValue = g.Sum(x => x.DispatchValue),
                    Paid = g.Sum(x => x.Paid),
                    AvailableReceivable = g.Sum(x => x.AvailableReceivable),
                    InvoiceDiscountAmount = g.Sum(x => x.InvoiceDiscountAmount),
                    ReturnValue = g.Sum(x => x.ReturnValue),
                    ReturnDiscountAmount = g.Sum(x => x.ReturnDiscountAmount)
                })
                .OrderBy(x => x.SaleInvoiceNo)
                .ToList();

            return mergedSummary;
        }

    }
}


