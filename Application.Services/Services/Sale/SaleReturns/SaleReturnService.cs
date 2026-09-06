using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Sale.SaleReturn;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleReturn;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Sale.SaleReturns
{
    public class SaleReturnService : BaseService<SaleReturn, SaleReturnCreationDto, SaleReturnUpdateDto, SaleReturnRequestModel, SaleReturnViewModel>, ISaleReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;

        public SaleReturnService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
            IHubContext<BroadcastHub, IHubClient> hubContext, IAccountService accountService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
        }

        public async Task<SaleReturnAggregatorModel> PrepareSaleReturnAggregatorModel(SaleReturnRequestModel saleReturnRequest)
        {
            var saleReturnQueryable = _unitOfWork.Repository<SaleReturn>().TableNoTracking().Where(saleReturnRequest.GetExpression());
            return new SaleReturnAggregatorModel
            {
                AggregatorSubtotal = await saleReturnQueryable.SumAsync(x => x.Subtotal),
                AggregatorTotalPercentageDiscountAmount = await saleReturnQueryable.SumAsync(x => x.TotalPercentageDiscountAmount),
                AggregatorOtherDiscount = await saleReturnQueryable.SumAsync(x => x.OtherDiscount),
                AggregatorTotal = await saleReturnQueryable.SumAsync(x => x.Total),
            };
        }

        public async Task<SaleReturnViewModel> GetByIdAsync(Guid id)
        {
            var saleReturn = await _unitOfWork.Repository<SaleReturn>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Customer).Include(x => x.Store)
                .Include(x => x.SaleReturnDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (saleReturn == null) throw new NotFoundResultException("Sale Return Not Found With this Id");
            return _mapper.Map<SaleReturnViewModel>(saleReturn);
        }

        public new async Task<Guid> AddAsync(SaleReturnCreationDto saleReturnCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            saleReturnCreationDto.SaleReturnDate = saleReturnCreationDto.SaleReturnDate.ToLocal();
            var saleReturn = _mapper.Map<SaleReturn>(saleReturnCreationDto);
            var count = _unitOfWork.Repository<SaleReturn>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            saleReturn.Id = Guid.NewGuid();
            saleReturn.SaleReturnNo = "SR" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            saleReturn.FinancialYearId = financialYear.Id;
            saleReturn.Status = (int)SaleReturnStatus.Pending;

            var totalVat = 0M;
            foreach (var item in saleReturn.SaleReturnDetails)
            {
                if (item.ReturnPrimaryQuantity == 0) continue;
                item.Id = Guid.NewGuid();
                item.SaleReturnId = saleReturn.Id;
                //Vat Calculation start
                totalVat += VatCalculator.CalculateVat(item.VatPercentage, (item.ReturnQuantity * item.Rate));
                //Vat Calculation End
                await _unitOfWork.Repository<SaleReturnDetail>().AddAsync(item);
            }
            saleReturn.TotalVat = totalVat;
            saleReturn.SaleReturnDetails = null;
            await _unitOfWork.Repository<SaleReturn>().AddAsync(saleReturn);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, saleReturn.Id
                , "/sales/sale-return/" + saleReturn.Id, Permissions.SaleReturns.Check,
                "Sale Return " + saleReturn.SaleReturnNo + " is ready for Check", (int)SaleReturnStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return saleReturn.Id;
        }

        public new async Task<Guid> UpdateAsync(SaleReturnUpdateDto saleReturnUpdateDto)
        {
            var dbSaleReturn = await _unitOfWork.Repository<SaleReturn>().FindAsync(saleReturnUpdateDto.Id);
            //when date updated,convert it to local date
            if (saleReturnUpdateDto.SaleReturnDate.Equals(dbSaleReturn.SaleReturnDate) == false)
            {
                saleReturnUpdateDto.SaleReturnDate = saleReturnUpdateDto.SaleReturnDate.ToLocal();
            }
            var saleReturn = _mapper.Map(saleReturnUpdateDto, dbSaleReturn);
            if (dbSaleReturn.Status == (int)SaleReturnStatus.Pending || dbSaleReturn.Status == (int)SaleReturnStatus.Checked)
            {
                var totalVat = 0M;
                foreach (var item in saleReturn.SaleReturnDetails)
                {
                    //Vat Calculation start
                    totalVat += VatCalculator.CalculateVat(item.VatPercentage, (item.ReturnQuantity * item.Rate));
                    //Vat Calculation End
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.SaleReturnId = saleReturn.Id;
                        await _unitOfWork.Repository<SaleReturnDetail>().AddAsync(item);
                    }
                    else
                    {
                        await _unitOfWork.Repository<SaleReturnDetail>().UpdateAsync(item);
                    }

                }
                saleReturn.TotalVat = totalVat;
                await _unitOfWork.Repository<SaleReturn>().UpdateAsync(saleReturn);
                //Delete for SaleReturnDetails
                if (!string.IsNullOrEmpty(saleReturnUpdateDto.DeletedSaleReturnDetailIds))
                {
                    foreach (var id in saleReturnUpdateDto.DeletedSaleReturnDetailIds.Split(',').Where(x => x != ""))
                    {
                        var saleReturnDetail = await _unitOfWork.Repository<SaleReturnDetail>().FindAsync(new Guid(id));
                        saleReturnDetail.Deleted = true;
                        await _unitOfWork.Repository<SaleReturnDetail>().UpdateAsync(saleReturnDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return saleReturn.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var saleReturn = await _unitOfWork.Repository<SaleReturn>().FindAsync(x => x.Id == id, x => x.Include(x => x.SaleReturnDetails));
            if (saleReturn == null) throw new NotFoundResultException("Sale Return Not Found With this Id");
            if (saleReturn.Status == (int)SaleReturnStatus.Pending)
            {
                foreach (var item in saleReturn.SaleReturnDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<SaleReturnDetail>().UpdateAsync(item);
                }
                saleReturn.Deleted = true;
                await _unitOfWork.Repository<SaleReturn>().UpdateAsync(saleReturn);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return saleReturn.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var saleReturns = _unitOfWork.Repository<SaleReturn>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await saleReturns.Where(x => x.Status == (int)SaleReturnStatus.Pending).CountAsync(),
                CheckedCount = await saleReturns.Where(x => x.Status == (int)SaleReturnStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbSaleReturn = await _unitOfWork.Repository<SaleReturn>().FindAsync(id);
            if (dbSaleReturn.Status == (int)SaleReturnStatus.Pending)
            {
                dbSaleReturn.Status = (int)SaleReturnStatus.Checked;
                dbSaleReturn.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<SaleReturn>().UpdateAsync(dbSaleReturn);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSaleReturn.Id
                       && x.Status == (int)SaleReturnStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbSaleReturn.Id
                    , "/sales/sale-return/" + dbSaleReturn.Id, Permissions.SaleReturns.Approve,
                    "Sale Return " + dbSaleReturn.SaleReturnNo + " is ready for Approval", (int)SaleReturnStatus.Checked);
                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Sale Return Status has already been checked by " + dbSaleReturn?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbSaleReturnInfo = await _unitOfWork.Repository<SaleReturn>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbSaleReturnInfo == null)
                throw new NotFoundResultException("Sale Return not found.");

            if (financialYear.Id != dbSaleReturnInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Sale Return does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<SaleReturn>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)SaleReturnStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)SaleReturnStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Sale Return already approved by {dbSaleReturnInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbSaleReturn = await _unitOfWork.Repository<SaleReturn>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.SaleReturnDetails));

            dbSaleReturn.Status = (int)SaleReturnStatus.Approved;
            dbSaleReturn.ApprovedBy = approvedBy;

            var cogs_Total = 0.0M;
            if (string.IsNullOrWhiteSpace(dbSaleReturn.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(dbSaleReturn.InvoiceNo))
            {
                var saleInvoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(x => x.InvoiceNo.Contains(dbSaleReturn.InvoiceNo));
                foreach (var item in dbSaleReturn.SaleReturnDetails)
                {
                    if (item.ReturnQuantity == 0) continue;
                    decimal cogs = await InsertStock(dbSaleReturn, saleInvoice.Id, item.ProductId, (item.ReturnQuantity + item.ReturnBonusQuantity));
                    cogs_Total += cogs;

                    //discount
                    var saleInvoiceDetails = await _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Where(x => x.SaleInvoiceId == saleInvoice.Id).ToListAsync();
                    var saleInvoiceDetail = saleInvoiceDetails.SingleOrDefault(x => x.ProductId == item.ProductId);
                    if (saleInvoiceDetail is not null)
                    {
                        item.MonthlyDiscountPerUnit = saleInvoiceDetail.MonthlyDiscountPerUnit;
                        item.YearlyDiscountPerUnit = saleInvoiceDetail.YearlyDiscountPerUnit;
                        item.TargetDiscountPerUnit = saleInvoiceDetail.TargetDiscountPerUnit;
                        //return qty in sale invoice detail table
                        saleInvoiceDetail.ReturnQuantity += item.ReturnQuantity;
                        saleInvoiceDetail.ReturnAmount += item.Amount;
                        await _unitOfWork.Repository<SaleInvoiceDetail>().UpdateAsync(saleInvoiceDetail);
                    }

                    await _unitOfWork.Repository<SaleReturnDetail>().UpdateAsync(item);
                }
            }
            else
            {
                var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking().SingleOrDefaultAsync(x => x.DeliveryNoteNo == dbSaleReturn.DeliveryNoteNo);
                if (deliveryNote is null) throw new NotFoundResultException("Delivery Note Not Found With this delivery note no");
                foreach (var item in dbSaleReturn.SaleReturnDetails)
                {
                    if (item.ReturnQuantity == 0) continue;
                    decimal cogs = await InsertStock(dbSaleReturn, deliveryNote.Id, item.ProductId, (item.ReturnQuantity + item.ReturnBonusQuantity));
                    cogs_Total += cogs;
                    //discount
                    var saleInvoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(x => x.DeliveryNoteNo.Contains(dbSaleReturn.DeliveryNoteNo));
                    var saleInvoiceDetails = await _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Where(x => x.SaleInvoiceId == saleInvoice.Id).ToListAsync();
                    var saleInvoiceDetail = saleInvoiceDetails.SingleOrDefault(x => x.ProductId == item.ProductId && x.DeliveryNoteNo == dbSaleReturn.DeliveryNoteNo);
                    if (saleInvoiceDetail is not null)
                    {
                        item.MonthlyDiscountPerUnit = saleInvoiceDetail.MonthlyDiscountPerUnit;
                        item.YearlyDiscountPerUnit = saleInvoiceDetail.YearlyDiscountPerUnit;
                        item.TargetDiscountPerUnit = saleInvoiceDetail.TargetDiscountPerUnit;
                        //return qty in sale invoice detail table
                        saleInvoiceDetail.ReturnQuantity += item.ReturnQuantity;
                        saleInvoiceDetail.ReturnAmount += item.Amount;
                        await _unitOfWork.Repository<SaleInvoiceDetail>().UpdateAsync(saleInvoiceDetail);
                    }

                    await _unitOfWork.Repository<SaleReturnDetail>().UpdateAsync(item);
                }
            }
            await InsertTransaction(dbSaleReturn, cogs_Total);

            //update Sale Return
            await _unitOfWork.Repository<SaleReturn>().UpdateAsync(dbSaleReturn);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSaleReturn.Id
                   && x.Status == (int)SaleReturnStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }

        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Load minimal data (no tracking)
            var dbInfo = await _unitOfWork.Repository<SaleReturn>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Sale Return not found.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Sale Return does not belong to the current financial year.");
            if (dbInfo.Status != fromStatus) throw new BadRequestException("Sale Return status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)SaleReturnStatus.Checked && dbInfo.Status != (int)SaleReturnStatus.Approved)
                throw new BadRequestException("Only Checked or Approved Sale Return can be unposted.");

            var newStatus = dbInfo.Status == (int)SaleReturnStatus.Approved
                ? (int)SaleReturnStatus.Checked
                : (int)SaleReturnStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<SaleReturn>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Sale Return status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbSaleReturn = await _unitOfWork.Repository<SaleReturn>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.SaleReturnDetails));

            dbSaleReturn.Status = newStatus;
            dbSaleReturn.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)SaleReturnStatus.Checked:
                    dbSaleReturn.CheckedBy = "";
                    break;
                case (int)SaleReturnStatus.Approved:
                    dbSaleReturn.ApprovedBy = "";
                    //reverse return qty and amount in sale invoice detail
                    if (string.IsNullOrWhiteSpace(dbSaleReturn.DeliveryNoteNo) && !string.IsNullOrWhiteSpace(dbSaleReturn.InvoiceNo))
                    {
                        foreach (var item in dbSaleReturn.SaleReturnDetails)
                        {
                            var saleInvoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(x => x.InvoiceNo.Contains(dbSaleReturn.InvoiceNo));
                            var saleInvoiceDetails = await _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Where(x => x.SaleInvoiceId == saleInvoice.Id).ToListAsync();
                            var saleInvoiceDetail = saleInvoiceDetails.SingleOrDefault(x => x.ProductId == item.ProductId);
                            if (saleInvoiceDetail is not null)
                            {
                                saleInvoiceDetail.ReturnQuantity -= item.ReturnQuantity;
                                saleInvoiceDetail.ReturnAmount -= item.Amount;
                                await _unitOfWork.Repository<SaleInvoiceDetail>().UpdateAsync(saleInvoiceDetail);
                            }

                            await _unitOfWork.Repository<SaleReturnDetail>().UpdateAsync(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbSaleReturn.SaleReturnDetails)
                        {
                            var saleInvoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(x => x.DeliveryNoteNo.Contains(dbSaleReturn.DeliveryNoteNo));
                            var saleInvoiceDetails = await _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Where(x => x.SaleInvoiceId == saleInvoice.Id).ToListAsync();
                            var saleInvoiceDetail = saleInvoiceDetails.SingleOrDefault(x => x.ProductId == item.ProductId && x.DeliveryNoteNo == dbSaleReturn.DeliveryNoteNo);
                            if (saleInvoiceDetail is not null)
                            {
                                saleInvoiceDetail.ReturnQuantity -= item.ReturnQuantity;
                                saleInvoiceDetail.ReturnAmount -= item.Amount;
                                await _unitOfWork.Repository<SaleInvoiceDetail>().UpdateAsync(saleInvoiceDetail);
                            }

                            await _unitOfWork.Repository<SaleReturnDetail>().UpdateAsync(item);
                        }
                    }
                    await DeleteStock(dbSaleReturn);
                    await DeleteTransaction(dbSaleReturn.SaleReturnNo);
                    break;
            }

            await _unitOfWork.Repository<SaleReturn>().UpdateAsync(dbSaleReturn);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbSaleReturn.Status;
        }

        private async Task<decimal> InsertStock(SaleReturn saleReturn, Guid transactionId, Guid productId, int returnQty)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var product = await _unitOfWork.Repository<Product>().FindAsync(productId);
            var stock = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .SingleOrDefaultAsync(x => x.TransactonType == (int)TransactonType.Sale && x.TransactionId == transactionId && x.ProductId == productId);
            if (stock is null) throw new NotFoundResultException("Stock  Not Found");
            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.SaleReturn,
                InventoryTypeId = product.InventoryTypeId,
                ProductTypeId = product.ProductTypeId,
                ProductId = stock.ProductId,
                StoreId = saleReturn.StoreId,
                AvailableQty = returnQty,
                InQty = returnQty,
                InRate = stock.OutRate,
                BatchNo = stock.BatchNo,
                TransactionId = saleReturn.Id,
                StockInDate = saleReturn.SaleReturnDate,
                TransactionDate = saleReturn.SaleReturnDate,
                Remark = "Sale Return#" + saleReturn?.SaleReturnNo,
                FinancialYearId = financialYear.Id
            });
            return returnQty * stock.OutRate;
        }

        private async Task DeleteStock(SaleReturn saleReturn)
        {
            var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.SaleReturn && x.TransactionId == saleReturn.Id).ToListAsync();
            if (stocks.Any())
            {
                foreach (var stock in stocks)
                {
                    if (stock.AvailableQty == stock.InQty)
                    {
                        await _unitOfWork.Repository<Stock>().DeleteAsync(stock.Id);
                    }
                    else throw new BadRequestException("Stock Already Occupied ! Not Possible to Unpost");
                }
            }
        }

        private async Task InsertTransaction(SaleReturn saleReturn, decimal cogsTotal)
        {

            StringBuilder sb = new StringBuilder();
            //sb.Append("Sale Return-");
            var transactionQty = 0;
            var transactionQtyValue = 0m;
            var totalMonthlyDiscount = 0m;
            var totalYearlyDiscount = 0m;
            var totalTargetDiscount = 0m;
            foreach (var item in saleReturn!.SaleReturnDetails)
            {
                var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
                var measurementUnit = await _unitOfWork.Repository<MeasurementUnit>().FindAsync(x => x.Id == product.MeasurementUnitId);
                totalMonthlyDiscount += (item.ReturnQuantity * item.MonthlyDiscountPerUnit);
                totalYearlyDiscount += (item.ReturnQuantity * item.YearlyDiscountPerUnit);
                totalTargetDiscount += (item.ReturnQuantity * item.TargetDiscountPerUnit);
                transactionQty += item.ReturnQuantity;
                transactionQtyValue = saleReturn.Total;
                sb.Append(" ");
                sb.Append(product?.Name);
                sb.Append(", ");
                sb.Append(item?.ReturnQuantity);
                sb.Append(" ");
                sb.Append(measurementUnit?.Name);
                sb.Append(" @");
                sb.Append(item?.Rate);
            }
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var saleReturnAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.SaleReturn.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(saleReturnAccountName);
            contraAccountIdsForCredit.Append(AccountHeadConstants.SaleReturn);
            if (saleReturn.TotalVat > 0M)
            {
                var vatOnSalesPayableAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.VatOnSalesPayable.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForCredit.Append(", " + vatOnSalesPayableAccountName);
                contraAccountIdsForCredit.Append(", " + AccountHeadConstants.VatOnSalesPayable);
            }
            var finishedGoodsInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.FinishedGoodsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(", " + finishedGoodsInventoryAccountName);
            contraAccountIdsForCredit.Append(", " + AccountHeadConstants.FinishedGoodsInventory);
            var totalDiscountBooking = totalMonthlyDiscount + totalYearlyDiscount + totalTargetDiscount;
            if (totalDiscountBooking > 0m)
            {
                var commissionPayableAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.CommissionPayable.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForCredit.Append(", " + commissionPayableAccountName);
                contraAccountIdsForCredit.Append(", " + AccountHeadConstants.CommissionPayable);
            }
            var customerAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == saleReturn.CustomerId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(customerAccountName);
            contraAccountIdsForDebit.Append(saleReturn.CustomerId.ToString());
            var cogsStandardAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.COGS_Standard.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(", " + cogsStandardAccountName);
            contraAccountIdsForDebit.Append(", " + AccountHeadConstants.COGS_Standard);
            if (totalMonthlyDiscount > 0m)
            {
                var monthlyDiscountAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.MonthlyDiscount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForDebit.Append(", " + monthlyDiscountAccountName);
                contraAccountIdsForDebit.Append(", " + AccountHeadConstants.MonthlyDiscount);
            }
            if (totalYearlyDiscount > 0m)
            {
                var yearlyDiscountAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.YearlyDiscount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForDebit.Append(", " + yearlyDiscountAccountName);
                contraAccountIdsForDebit.Append(", " + AccountHeadConstants.YearlyDiscount);
            }
            if (totalTargetDiscount > 0m)
            {
                var targetDiscountAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.TargetDiscount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForDebit.Append(", " + targetDiscountAccountName);
                contraAccountIdsForDebit.Append(", " + AccountHeadConstants.TargetDiscount);
            }

            //Transaction Entries start here
            if (saleReturn.TotalVat > 0M)
            {
                //sale return account debit
                await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                     Guid.Parse(AccountHeadConstants.SaleReturn), (saleReturn.Total - saleReturn.TotalVat), 0, saleReturn.SaleReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

                //Vat on Sales Payable account debit
                await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                     Guid.Parse(AccountHeadConstants.VatOnSalesPayable), saleReturn.TotalVat, 0, saleReturn.SaleReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            }
            else
            {
                //sale return account debit
                await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                     Guid.Parse(AccountHeadConstants.SaleReturn), saleReturn.Total, 0, saleReturn.SaleReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            }

            //customer account credit
            await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                  saleReturn.CustomerId, 0, saleReturn.Total, saleReturn.SaleReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.SaleReturn, -transactionQty, -transactionQtyValue);

            //finished goods inventory account debit
            await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                 Guid.Parse(AccountHeadConstants.FinishedGoodsInventory), cogsTotal, 0, saleReturn.SaleReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

            //cogs standard account credit
            await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                  Guid.Parse(AccountHeadConstants.COGS_Standard), 0, cogsTotal, saleReturn.SaleReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

            //Commission booking(reverse entry)
            //monthly discount account credit
            if (totalMonthlyDiscount > 0m)
            {
                await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                Guid.Parse(AccountHeadConstants.MonthlyDiscount), 0m, totalMonthlyDiscount, saleReturn.SaleReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            }
            //yearly discount account credit
            if (totalYearlyDiscount > 0m)
            {
                await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                Guid.Parse(AccountHeadConstants.YearlyDiscount), 0m, totalYearlyDiscount, saleReturn.SaleReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            }
            //target discount account credit
            if (totalTargetDiscount > 0m)
            {
                await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                Guid.Parse(AccountHeadConstants.TargetDiscount), 0m, totalTargetDiscount, saleReturn.SaleReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            }
            //commission payable account debit
            if (totalDiscountBooking > 0m)
            {
                await _accountService.HitAccount(_unitOfWork, null, saleReturn.SaleReturnNo, sb.ToString(),
                Guid.Parse(AccountHeadConstants.CommissionPayable), totalDiscountBooking, 0m, saleReturn.SaleReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            }

        }

        private async Task DeleteTransaction(string saleReturnNo)
        {
            var saleReturn = await _unitOfWork.Repository<SaleReturn>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.SaleReturnNo == saleReturnNo);
            if (saleReturn is null) throw new NotFoundResultException("Sale Return Not Found With this Sale Return No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == saleReturn.SaleReturnNo);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }

        }


    }
}
