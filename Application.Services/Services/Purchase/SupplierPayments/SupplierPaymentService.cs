using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.ReceiveVoucher;
using Application.Services.Dtos.Purchase.SupplierPayment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.SupplierPayment;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services.Services.Purchase.SupplierPayments
{
    public class SupplierPaymentService : BaseService<SupplierPayment, SupplierPaymentCreationDto, SupplierPaymentUpdateDto, SupplierPaymentRequestModel, SupplierPaymentViewModel>, ISupplierPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;

        public SupplierPaymentService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
            IHubContext<BroadcastHub, IHubClient> hubContext, IAccountService accountService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
        }

        public async Task<SupplierPaymentAggregatorModel> PrepareSupplierPaymentAggregatorModel(SupplierPaymentRequestModel supplierPaymentRequest)
        {
            var supplierPaymentQueryable = _unitOfWork.Repository<SupplierPayment>().TableNoTracking().Where(supplierPaymentRequest.GetExpression());
            return new SupplierPaymentAggregatorModel
            {
                AggregatorTotalAmount = await supplierPaymentQueryable.SumAsync(x => x.TotalAmount)
            };
        }

        public async Task<SupplierPaymentViewModel> GetByIdAsync(Guid id)
        {
            var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.Supplier).Include(x => x.PaymentMode)
                .Include(x => x.SupplierPaymentDetails).ThenInclude(x => x.Account));
            if (supplierPayment == null) throw new NotFoundResultException("Supplier Payment Not Found With this Id");
            return _mapper.Map<SupplierPaymentViewModel>(supplierPayment);
        }

        public new async Task<Guid> AddAsync(SupplierPaymentCreationDto supplierPaymentCreationDto)
        {
            if (!string.IsNullOrEmpty(supplierPaymentCreationDto.TransactionNumber))
            {
                var existingTransaction = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().AnyAsync(x => x.TransactionNumber == supplierPaymentCreationDto.TransactionNumber);
                if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
            }
            var financialYear = _workContext.GetCurrentFinancialYear();
            supplierPaymentCreationDto.PaymentDate = supplierPaymentCreationDto.PaymentDate.ToLocal();
            var supplierPayment = _mapper.Map<SupplierPayment>(supplierPaymentCreationDto);
            var count = _unitOfWork.Repository<SupplierPayment>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            supplierPayment.Id = Guid.NewGuid();
            supplierPayment.Code = "SP" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            supplierPayment.FinancialYearId = financialYear.Id;
            supplierPayment.Status = (int)SupplierPaymentStatus.Pending;


            //start - check if available due
            if (supplierPayment.SupplierPaymentType == (int)SupplierPaymentType.Payment_Against_Purchase_Invoice)
            {
                List<AvailableBalanceCheckInfo> infoList = new();
                foreach (var item in supplierPayment.SupplierPaymentDetails)
                {
                    var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().SingleOrDefaultAsync(x => x.PurchaseInvoiceNo == item.PurchaseInvoiceNo);
                    if (purchaseInvoice is null) throw new NotFoundResultException("Purchase Invoice not found associated with this supplier payment");
                    var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().SingleOrDefaultAsync(x => x.Ponumber == purchaseInvoice.Ponumber);
                    if (purchaseOrder is null) throw new BadRequestException("PO not found associated with Purchase Invoice associated with Supplier Payment Detail");
                    AvailableBalanceCheckInfo newItem = new AvailableBalanceCheckInfo
                    {
                        PurchaseInvoiceNo = purchaseInvoice.PurchaseInvoiceNo,
                        PurchaseOrderNo = purchaseOrder.Ponumber,
                        PurchaseInvoiceId = purchaseInvoice.Id,
                        PurchaseOrderId = purchaseOrder.Id,
                        PayableAmount = item.Amount
                    };
                    infoList.Add(newItem);
                }
                await ValidatePaymentAgainstPurchaseInvoiceAsync(infoList);
            }
            //End - check if available due

            foreach (var item in supplierPayment.SupplierPaymentDetails)
            {
                item.Id = Guid.NewGuid();
                item.SupplierPaymentId = supplierPayment.Id;
                await _unitOfWork.Repository<SupplierPaymentDetail>().AddAsync(item);
            }

            await _unitOfWork.Repository<SupplierPayment>().AddAsync(supplierPayment);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, supplierPayment.Id
                , "/purchase/supplier-payment/" + supplierPayment.Id, Permissions.SupplierPayments.Check,
                "Supplier Payment " + supplierPayment.Code + " is ready for Check", (int)SupplierPaymentStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return supplierPayment.Id;
        }

        public async Task ValidatePaymentAgainstPurchaseInvoiceAsync(List<AvailableBalanceCheckInfo> infoList)
        {
            var groupedInfoList = infoList.GroupBy(x => x.PurchaseOrderNo).ToList();
            foreach (var groupedInfo in groupedInfoList)
            {
                var payableAmount = 0m;
                foreach (var item in groupedInfo)
                {
                    var supplierTransactionsAgainstPO = await _unitOfWork.Repository<SupplierTransactionAgainstPo>().TableNoTracking().Where(x => x.PurchaseOrderId == item.PurchaseOrderId).ToListAsync();
                    var dueAmount = supplierTransactionsAgainstPO.Sum(x => x.Amount);
                    payableAmount += item.PayableAmount;
                    if (payableAmount > dueAmount) throw new BadRequestException("Invalid Attempt! Due Amount Of " + item.PurchaseOrderNo + " is " + dueAmount);
                }
            }
        }

        public new async Task<Guid> UpdateAsync(SupplierPaymentUpdateDto supplierPaymentUpdateDto)
        {
            var dbSupplierPayment = await _unitOfWork.Repository<SupplierPayment>().FindAsync(supplierPaymentUpdateDto.Id);
            if (!string.IsNullOrEmpty(supplierPaymentUpdateDto.TransactionNumber))
            {
                if (supplierPaymentUpdateDto.TransactionNumber.Equals(dbSupplierPayment.TransactionNumber) == false)
                {
                    var existingTransaction = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().AnyAsync(x => x.TransactionNumber == supplierPaymentUpdateDto.TransactionNumber);
                    if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
                }
            }
            //when date updated,convert it to local date
            if (supplierPaymentUpdateDto.PaymentDate.Equals(dbSupplierPayment.PaymentDate) == false)
            {
                supplierPaymentUpdateDto.PaymentDate = supplierPaymentUpdateDto.PaymentDate.ToLocal();
            }
            if (dbSupplierPayment == null) throw new NotFoundResultException("SupplierPayment Not Found With this Id");
            var supplierPayment = _mapper.Map(supplierPaymentUpdateDto, dbSupplierPayment);
            if (dbSupplierPayment.Status == (int)SupplierPaymentStatus.Pending || dbSupplierPayment.Status == (int)SupplierPaymentStatus.Checked)
            {
                await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                foreach (var item in supplierPayment.SupplierPaymentDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.SupplierPaymentId = supplierPayment.Id;
                        await _unitOfWork.Repository<SupplierPaymentDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<SupplierPaymentDetail>().UpdateAsync(item);
                }
                //Delete for SupplierPaymentDetails
                if (!string.IsNullOrEmpty(supplierPaymentUpdateDto.DeletedSupplierPaymentDetailIds))
                {
                    foreach (var id in supplierPaymentUpdateDto.DeletedSupplierPaymentDetailIds.Split(',').Where(x => x != ""))
                    {
                        var supplierPaymentDetail = await _unitOfWork.Repository<SupplierPaymentDetail>().FindAsync(new Guid(id));
                        supplierPaymentDetail.Deleted = true;
                        await _unitOfWork.Repository<SupplierPaymentDetail>().UpdateAsync(supplierPaymentDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return supplierPayment.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().FindAsync(x => x.Id == id, x => x.Include(x => x.SupplierPaymentDetails));
            if (supplierPayment == null) throw new NotFoundResultException("SupplierPayment Not Found With this Id");
            if (supplierPayment.Status == (int)SupplierPaymentStatus.Pending)
            {
                foreach (var item in supplierPayment.SupplierPaymentDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<SupplierPaymentDetail>().UpdateAsync(item);
                }
                supplierPayment.Deleted = true;
                await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");
            return supplierPayment.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var supplierPayments = _unitOfWork.Repository<SupplierPayment>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await supplierPayments.Where(x => x.Status == (int)SupplierPaymentStatus.Pending).CountAsync(),
                CheckedCount = await supplierPayments.Where(x => x.Status == (int)SupplierPaymentStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbSupplierPayment = await _unitOfWork.Repository<SupplierPayment>().FindAsync(id);
            dbSupplierPayment.Status = (int)SupplierPaymentStatus.Checked;
            dbSupplierPayment.CheckedBy = _workContext.GetUserName();
            //update SupplierPayment
            await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(dbSupplierPayment);
            //remove pending notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSupplierPayment.Id
                   && x.Status == (int)SupplierPaymentStatus.Pending);
            //send notification for approval
            await _notificationService.SendNotificationAsync(_unitOfWork, dbSupplierPayment.Id
                , "/purchase/supplier-payment/" + dbSupplierPayment.Id, Permissions.SupplierPayments.Approve,
                "Supplier Payment " + dbSupplierPayment.Code + " is ready for Approval", (int)SupplierPaymentStatus.Checked);

            bool isChecked = await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            return isChecked;
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<SupplierPayment>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Supplier Payment not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Supplier Payment does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<SupplierPayment>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)SupplierPaymentStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)SupplierPaymentStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Supplier Payment already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbSupplierPayment = await _unitOfWork.Repository<SupplierPayment>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.SupplierPaymentDetails).Include(x => x.Supplier));

            dbSupplierPayment.Status = (int)SupplierPaymentStatus.Approved;
            dbSupplierPayment.ApprovedBy = approvedBy;

            await InsertTransaction(dbSupplierPayment);
            await InsertSupplierTransactionAgainstPO(dbSupplierPayment);
            //update SupplierPayment
            await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(dbSupplierPayment);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSupplierPayment.Id
                   && x.Status == (int)SupplierPaymentStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<SupplierPayment>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Supplier Payment not found.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Supplier Payment does not belong to the current financial year.");
            if (dbInfo.Status != fromStatus) throw new BadRequestException("Supplier Payment status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)SupplierPaymentStatus.Checked && dbInfo.Status != (int)SupplierPaymentStatus.Approved)
                throw new BadRequestException("Only Checked or Approved Supplier Payment can be unposted.");

            var newStatus = dbInfo.Status == (int)SupplierPaymentStatus.Approved
                ? (int)SupplierPaymentStatus.Checked
                : (int)SupplierPaymentStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<SupplierPayment>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Supplier Payment status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbSupplierPayment = await _unitOfWork.Repository<SupplierPayment>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.SupplierPaymentDetails).Include(x => x.Supplier));

            dbSupplierPayment.Status = newStatus;
            dbSupplierPayment.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)SupplierPaymentStatus.Checked:
                    dbSupplierPayment.CheckedBy = "";
                    break;
                case (int)SupplierPaymentStatus.Approved:
                    dbSupplierPayment.ApprovedBy = "";
                    foreach (var supplierPaymentDetail in dbSupplierPayment.SupplierPaymentDetails)
                    {
                        await ReducePurchaseInvoicePayableAmountAndUpdateStatus(supplierPaymentDetail);
                    }
                    await DeleteTransaction(dbSupplierPayment.Code);
                    await RemoveSupplierTransactionAgainstPO(dbSupplierPayment);
                    break;
            }

            await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(dbSupplierPayment);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbSupplierPayment.Status;
        }

        private async Task InsertTransaction(SupplierPayment supplierPayment)
        {
            if (supplierPayment!.SupplierPaymentDetails.Sum(x => x.Amount) != supplierPayment.TotalAmount) throw new BadRequestException("Debit Credit Not Equal!");
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            var supplierAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == supplierPayment.SupplierId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(supplierAccountName);
            contraAccountIdsForCredit.Append(supplierPayment.SupplierId.ToString());
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var groupByAccountData = supplierPayment.SupplierPaymentDetails.GroupBy(x => x.AccountId).Select(g => new
            {
                AccountId = g.Key,
                Amount = g.Sum(x => x.Amount)
            }).ToList();
            var counter = 0m;
            foreach (var item in groupByAccountData)
            {
                if (counter > 0)
                {
                    contraAccountNamesForDebit.Append(", ");
                    contraAccountIdsForDebit.Append(", ");
                }
                var accountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.AccountId).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForDebit.Append(accountName);
                contraAccountIdsForDebit.Append(item.AccountId.ToString());
                counter++;
            }

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(supplierPayment.Ponumber)) sb.Append(supplierPayment?.Ponumber);
            else
            {
                foreach (var item in supplierPayment.SupplierPaymentDetails)
                {
                    var dbPurchaseInvoiceRelatedToSupplierPayment = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(x => x.PurchaseInvoiceNo == item.PurchaseInvoiceNo);
                    sb.Append(item.PurchaseInvoiceNo)
                      .Append(" ")
                      .Append(dbPurchaseInvoiceRelatedToSupplierPayment.Ponumber)
                      .Append(",");
                }
            }
            sb.Append(" ");
            sb.Append(supplierPayment?.Remark);
            //cash/bank account credit
            foreach (var item in groupByAccountData)
            {
                await _accountService.HitAccount(_unitOfWork, supplierPayment!.CostCenterId, supplierPayment.Code, sb.ToString(),
                item.AccountId, 0.0M, item.Amount, supplierPayment.PaymentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            }
            //supplier account debit
            await _accountService.HitAccount(_unitOfWork, supplierPayment!.CostCenterId, supplierPayment.Code, sb.ToString(),
                supplierPayment.SupplierId, supplierPayment.TotalAmount, 0.0M, supplierPayment.PaymentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.SupplierPayment, 0, 0m, supplierPayment.PaymentModeId);
        }

        private async Task DeleteTransaction(string code)
        {
            var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (supplierPayment is null) throw new NotFoundResultException("Supplier Payment Not Found With this Code No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == supplierPayment.Code);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }
        }

        private async Task InsertSupplierTransactionAgainstPO(SupplierPayment supplierPayment)
        {

            switch (supplierPayment.SupplierPaymentType)
            {
                case (int)SupplierPaymentType.Advance:
                    var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().SingleOrDefaultAsync(x => x.Ponumber == supplierPayment.Ponumber);
                    if (purchaseOrder is null) throw new BadRequestException("PO not found associated with this Supplier Payment:" + supplierPayment.Code);
                    await _unitOfWork.Repository<SupplierTransactionAgainstPo>().AddAsync(new SupplierTransactionAgainstPo
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = supplierPayment.Id,
                        TransactionDate = supplierPayment.PaymentDate,
                        SupplierInvoiceDate = null,
                        PaymentTermInDays = 0,
                        PurchaseOrderId = purchaseOrder!.Id,
                        SupplierId = supplierPayment.SupplierId,
                        SupplierTransactionType = (int)SupplierTransactionType.Advance_Payment,
                        Amount = -supplierPayment.TotalAmount,
                        Remark = supplierPayment?.Remark,
                    });
                    break;
                case (int)SupplierPaymentType.Payment_Against_Purchase_Invoice:

                    List<AvailableBalanceCheckInfo> infoList = new();
                    foreach (var supplierPaymentDetail in supplierPayment.SupplierPaymentDetails)
                    {
                        var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().SingleOrDefaultAsync(x => x.PurchaseInvoiceNo == supplierPaymentDetail.PurchaseInvoiceNo);
                        if (purchaseInvoice is null) throw new NotFoundResultException("Purchase Invoice not found associated with this supplier payment");
                        var po = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().SingleOrDefaultAsync(x => x.Ponumber == purchaseInvoice.Ponumber);
                        if (po is null) throw new BadRequestException("PO not found associated with Purchase Invoice associated with Supplier Payment Detail");
                        //start - check available balance
                        AvailableBalanceCheckInfo newItem = new AvailableBalanceCheckInfo
                        {
                            PurchaseInvoiceNo = purchaseInvoice.PurchaseInvoiceNo,
                            PurchaseOrderNo = po.Ponumber,
                            PurchaseInvoiceId = purchaseInvoice.Id,
                            PurchaseOrderId = po.Id,
                            PayableAmount = supplierPaymentDetail.Amount
                        };
                        infoList.Add(newItem);
                        //end - check available balance
                        await IncreasePurchaseInvoicePayableAmountAndUpdateStatus(purchaseInvoice, supplierPaymentDetail.Amount);
                        await _unitOfWork.Repository<SupplierTransactionAgainstPo>().AddAsync(new SupplierTransactionAgainstPo
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = supplierPayment!.Id,
                            TransactionDate = supplierPayment.PaymentDate,
                            SupplierInvoiceDate = null,
                            PaymentTermInDays = 0,
                            PurchaseOrderId = po.Id,
                            SupplierId = supplierPayment.SupplierId,
                            SupplierTransactionType = (int)SupplierTransactionType.Payment_Against_Purchase_Invoice,
                            Amount = -supplierPaymentDetail.Amount,
                            Remark = supplierPayment?.Remark,
                        });
                    }
                    //start - check available balance 
                    await ValidatePaymentAgainstPurchaseInvoiceAsync(infoList);
                    //end - check available balance
                    break;
                default:
                    break;
            }
        }

        private async Task RemoveSupplierTransactionAgainstPO(SupplierPayment supplierPayment)
        {
            var transactions = _unitOfWork.Repository<SupplierTransactionAgainstPo>().TableNoTracking().Where(x => x.TransactionId == supplierPayment.Id);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                    await _unitOfWork.Repository<SupplierTransactionAgainstPo>().DeleteAsync(transaction.Id);
            }
        }

        private async Task ReducePurchaseInvoicePayableAmountAndUpdateStatus(SupplierPaymentDetail supplierPaymentDetail)
        {
            if (!string.IsNullOrWhiteSpace(supplierPaymentDetail.PurchaseInvoiceNo))
            {
                var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().SingleOrDefaultAsync(x => x.PurchaseInvoiceNo == supplierPaymentDetail.PurchaseInvoiceNo);
                if (purchaseInvoice is null) throw new NotFoundResultException("Purchase Invoice not found associated with this supplier payment");
                purchaseInvoice.PaidAmount -= supplierPaymentDetail.Amount;
                purchaseInvoice.Status = GetPurchaseInvoiceStatus(purchaseInvoice);
                await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(purchaseInvoice);
            }

        }

        private async Task IncreasePurchaseInvoicePayableAmountAndUpdateStatus(PurchaseInvoice purchaseInvoice, decimal amount)
        {
            purchaseInvoice.PaidAmount += amount;
            purchaseInvoice.Status = GetPurchaseInvoiceStatus(purchaseInvoice);
            await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(purchaseInvoice);

        }

        private int GetPurchaseInvoiceStatus(PurchaseInvoice purchaseInvoice)
        {
            //var advancePayments = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().Where(x => x.SupplierPaymentType == (int)SupplierPaymentType.Advance
            //  && x.Ponumber == purchaseInvoice.Ponumber).ToListAsync();
            var advancePaymentAmount = 0m;
            // if (advancePayments.Any()) advancePaymentAmount += advancePayments.Sum(x => x.TotalAmount);
            var paidAmount = advancePaymentAmount + purchaseInvoice.PaidAmount;
            if (paidAmount > purchaseInvoice.NetPayable) throw new BadRequestException("paid amount (including advance) can not be greater than payable amount");
            if (paidAmount == purchaseInvoice.NetPayable) return (int)PurchaseInvoiceStatus.Fully_Paid;
            else if (paidAmount == 0m) return (int)PurchaseInvoiceStatus.Approved;
            else return (int)PurchaseInvoiceStatus.Partially_Paid;
        }

        private async Task<int> GetPurchaseInvoiceStatus(PurchaseInvoice purchaseInvoice, PurchaseOrder purchaseOrder, SupplierPayment supplierPayment)
        {
            var supplierTransactionsAgainstPO = await _unitOfWork.Repository<SupplierTransactionAgainstPo>().TableNoTracking().Where(x =>
                x.PurchaseOrderId == purchaseOrder.Id).ToListAsync();
            var due = supplierTransactionsAgainstPO.Sum(x => x.Amount);
            if (supplierPayment.TotalAmount > due) throw new BadRequestException("Invalid Attempt!! Due Amount Of " + supplierPayment.Ponumber + " is " + due);
            if (supplierPayment.TotalAmount == due) return (int)PurchaseInvoiceStatus.Fully_Paid;
            else if (purchaseInvoice.PaidAmount == 0m) return (int)PurchaseInvoiceStatus.Approved;
            else return (int)PurchaseInvoiceStatus.Partially_Paid;
        }

    }
}
