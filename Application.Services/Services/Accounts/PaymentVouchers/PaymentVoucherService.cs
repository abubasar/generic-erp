using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.PaymentVoucher;
using Application.Services.Dtos.Accounts.ReceiveVoucher;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.PaymentVoucher;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Accounts.PaymentVouchers
{
    public class PaymentVoucherService : BaseService<PaymentVoucher, PaymentVoucherCreationDto, PaymentVoucherUpdateDto, PaymentVoucherRequestModel, PaymentVoucherViewModel>, IPaymentVoucherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ISmsService _smsService;

        public PaymentVoucherService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
            INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext,
            IAccountService accountService, ISmsService smsService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
            _smsService = smsService;
        }

        public virtual async Task<PaymentVoucherAggregatorModel> PreparePaymentVoucherAggregatorModel(PaymentVoucherRequestModel paymentVoucherRequestModel)
        {
            var paymentVoucherQueryable = _unitOfWork.Repository<PaymentVoucher>().TableNoTracking().Where(paymentVoucherRequestModel.GetExpression());
            return new PaymentVoucherAggregatorModel
            {
                AggregatorTotalAmount = await paymentVoucherQueryable.SumAsync(x => x.TotalAmount)
            };
        }

        public async Task<PaymentVoucherViewModel> GetByIdAsync(Guid id)
        {
            var paymentVoucher = await _unitOfWork.Repository<PaymentVoucher>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.CashBankAccount).Include(x => x.PaymentMode).Include(blog => blog.PaymentVoucherDetails).ThenInclude(x => x.Account));
            if (paymentVoucher == null) throw new NotFoundResultException("Payment Voucher Not Found With this Id");
            return _mapper.Map<PaymentVoucherViewModel>(paymentVoucher);
        }

        public new async Task<Guid> AddAsync(PaymentVoucherCreationDto paymentVoucherCreationDto)
        {
            if (!string.IsNullOrEmpty(paymentVoucherCreationDto.TransactionNumber))
            {
                var existingTransaction = await _unitOfWork.Repository<PaymentVoucher>().TableNoTracking().AnyAsync(x => x.TransactionNumber == paymentVoucherCreationDto.TransactionNumber);
                if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
            }
            var financialYear = _workContext.GetCurrentFinancialYear();
            paymentVoucherCreationDto.VoucherDate = paymentVoucherCreationDto.VoucherDate.ToLocal();
            var paymentVoucher = _mapper.Map<PaymentVoucher>(paymentVoucherCreationDto);
            var queryable = _unitOfWork.Repository<PaymentVoucher>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters();
            var costCenter = await _unitOfWork.Repository<CostCenter>().FindAsync(paymentVoucherCreationDto.CostCenterId);
            if (costCenter is null) throw new BadRequestException("Please Select Cost Center");
            var paymentVoucherCount = queryable.Where(x => x.CostCenterId == costCenter.Id).Count() + 1;
            paymentVoucher.VoucherNo = "CPV" + costCenter.Name.Substring(0, 1) + financialYear.Code + "-" + paymentVoucherCount.ToString().PadLeft(7, '0');
            paymentVoucher.Id = Guid.NewGuid();
            paymentVoucher.FinancialYearId = financialYear.Id;
            paymentVoucher.Status = (int)PaymentVoucherStatus.Pending;
            foreach (var item in paymentVoucher.PaymentVoucherDetails)
            {
                item.Id = Guid.NewGuid();
                item.PaymentVoucherId = paymentVoucher.Id;
                await _unitOfWork.Repository<PaymentVoucherDetail>().AddAsync(item);
            }

            await _unitOfWork.Repository<PaymentVoucher>().AddAsync(paymentVoucher);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, paymentVoucher.Id
                , "/accounts/payment-voucher/" + paymentVoucher.Id, Permissions.PaymentVouchers.Check,
                "Payment Voucher " + paymentVoucher.VoucherNo + " is ready for Check", (int)PaymentVoucherStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return paymentVoucher.Id;
        }

        public new async Task<Guid> UpdateAsync(PaymentVoucherUpdateDto paymentVoucherUpdateDto)
        {
            var dbPaymentVoucher = await _unitOfWork.Repository<PaymentVoucher>().FindAsync(paymentVoucherUpdateDto.Id);
            if (!string.IsNullOrEmpty(paymentVoucherUpdateDto.TransactionNumber))
            {
                if (paymentVoucherUpdateDto.TransactionNumber.Equals(dbPaymentVoucher.TransactionNumber) == false)
                {
                    var existingTransaction = await _unitOfWork.Repository<PaymentVoucher>().TableNoTracking().AnyAsync(x => x.TransactionNumber == paymentVoucherUpdateDto.TransactionNumber);
                    if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
                }
            }
            //when date updated,convert it to local date
            if (paymentVoucherUpdateDto.VoucherDate.Equals(dbPaymentVoucher.VoucherDate) == false)
            {
                paymentVoucherUpdateDto.VoucherDate = paymentVoucherUpdateDto.VoucherDate.ToLocal();
            }
            var paymentVoucher = _mapper.Map(paymentVoucherUpdateDto, dbPaymentVoucher);
            if (dbPaymentVoucher.Status == (int)PaymentVoucherStatus.Pending || dbPaymentVoucher.Status == (int)PaymentVoucherStatus.Checked)
            {
                await _unitOfWork.Repository<PaymentVoucher>().UpdateAsync(paymentVoucher);
                foreach (var item in paymentVoucher.PaymentVoucherDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.PaymentVoucherId = paymentVoucher.Id;
                        await _unitOfWork.Repository<PaymentVoucherDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<PaymentVoucherDetail>().UpdateAsync(item);
                }
                //Delete for PaymentVoucherDetails
                if (!string.IsNullOrEmpty(paymentVoucherUpdateDto.DeletedPaymentVoucherDetailIds))
                {
                    foreach (var id in paymentVoucherUpdateDto.DeletedPaymentVoucherDetailIds.Split(',').Where(x => x != ""))
                    {
                        var paymentVoucherDetail = await _unitOfWork.Repository<PaymentVoucherDetail>().FindAsync(new Guid(id));
                        paymentVoucherDetail.Deleted = true;
                        await _unitOfWork.Repository<PaymentVoucherDetail>().UpdateAsync(paymentVoucherDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return paymentVoucher.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var paymentVoucher = await _unitOfWork.Repository<PaymentVoucher>().FindAsync(x => x.Id == id, x => x.Include(x => x.PaymentVoucherDetails));
            if (paymentVoucher == null) throw new NotFoundResultException("PaymentVoucher Not Found With this Id");
            if (paymentVoucher.Status == (int)PaymentVoucherStatus.Pending)
            {
                foreach (var item in paymentVoucher.PaymentVoucherDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<PaymentVoucherDetail>().UpdateAsync(item);
                }
                paymentVoucher.Deleted = true;
                await _unitOfWork.Repository<PaymentVoucher>().UpdateAsync(paymentVoucher);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return paymentVoucher.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var paymentVouchers = _unitOfWork.Repository<PaymentVoucher>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await paymentVouchers.Where(x => x.Status == (int)PaymentVoucherStatus.Pending).CountAsync(),
                CheckedCount = await paymentVouchers.Where(x => x.Status == (int)PaymentVoucherStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbPaymentVoucher = await _unitOfWork.Repository<PaymentVoucher>().FindAsync(id);
            if (dbPaymentVoucher.Status == (int)PaymentVoucherStatus.Pending)
            {
                dbPaymentVoucher.Status = (int)PaymentVoucherStatus.Checked;
                dbPaymentVoucher.CheckedBy = _workContext.GetUserName();
                //update PaymentVoucher
                await _unitOfWork.Repository<PaymentVoucher>().UpdateAsync(dbPaymentVoucher);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPaymentVoucher.Id
                       && x.Status == (int)PaymentVoucherStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbPaymentVoucher.Id
                    , "/accounts/payment-voucher/" + dbPaymentVoucher.Id, Permissions.PaymentVouchers.Approve,
                    "Payment Voucher " + dbPaymentVoucher.VoucherNo + " is ready for Approval", (int)PaymentVoucherStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Payment Voucher Status has already been checked by " + dbPaymentVoucher?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<PaymentVoucher>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Payment Voucher not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Payment Voucher does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<PaymentVoucher>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)PaymentVoucherStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)PaymentVoucherStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Payment Voucher already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbPaymentVoucher = await _unitOfWork.Repository<PaymentVoucher>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.PaymentVoucherDetails).ThenInclude(x => x.Account));

            dbPaymentVoucher.Status = (int)PaymentVoucherStatus.Approved;
            dbPaymentVoucher.ApprovedBy = approvedBy;

            await InsertTransaction(dbPaymentVoucher);
            //update PaymentVoucher
            await _unitOfWork.Repository<PaymentVoucher>().UpdateAsync(dbPaymentVoucher);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPaymentVoucher.Id
                   && x.Status == (int)PaymentVoucherStatus.Checked);

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

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<PaymentVoucher>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("PaymentVoucher Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Payment Voucher does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)PaymentVoucherStatus.Checked && dbInfo.Status != (int)PaymentVoucherStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)PaymentVoucherStatus.Checked
                ? (int)PaymentVoucherStatus.Pending
                : (int)PaymentVoucherStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<PaymentVoucher>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbPaymentVoucher = await _unitOfWork.Repository<PaymentVoucher>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.PaymentVoucherDetails));
            dbPaymentVoucher.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)PaymentVoucherStatus.Checked:
                    dbPaymentVoucher.CheckedBy = "";
                    break;
                case (int)PaymentVoucherStatus.Approved:
                    dbPaymentVoucher.ApprovedBy = "";
                    await DeleteTransaction(dbPaymentVoucher.VoucherNo);
                    break;
            }

            await _unitOfWork.Repository<PaymentVoucher>().UpdateAsync(dbPaymentVoucher);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbPaymentVoucher.Status;
        }

        private async Task InsertTransaction(PaymentVoucher paymentVoucher)
        {
            if (paymentVoucher.PaymentVoucherDetails.Sum(x => x.Amount) != paymentVoucher.TotalAmount) throw new BadRequestException("Debit Credit Not Equal!");
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var cashBankAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == paymentVoucher.CashBankAccountId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(cashBankAccountName);
            contraAccountIdsForDebit.Append(paymentVoucher.CashBankAccountId.ToString());
            var counter = 0M;
            foreach (var item in paymentVoucher.PaymentVoucherDetails)
            {
                var accountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.AccountId).Select(x => x.Name).FirstOrDefaultAsync();
                if (counter > 0)
                {
                    contraAccountNamesForCredit.Append(", ");
                    contraAccountIdsForCredit.Append(", ");
                }
                contraAccountNamesForCredit.Append(accountName);
                contraAccountIdsForCredit.Append(item.AccountId.ToString());
                counter++;
            }
            //Transaction entries start here
            foreach (var item in paymentVoucher.PaymentVoucherDetails)
            {
                if (item?.Account.ParentId == AccountHeadConstants.TradePayable.ToGuid())
                {
                    await _accountService.HitAccount(_unitOfWork, paymentVoucher.CostCenterId, paymentVoucher.VoucherNo, paymentVoucher.Remark,
                     item!.AccountId, item.Amount, 0, paymentVoucher.VoucherDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.SupplierPayment, 0, 0m, paymentVoucher.PaymentModeId);
                }
                else
                {
                    await _accountService.HitAccount(_unitOfWork, paymentVoucher.CostCenterId, paymentVoucher.VoucherNo, paymentVoucher.Remark,
                     item!.AccountId, item.Amount, 0, paymentVoucher.VoucherDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.OtherPayment, 0, 0m, paymentVoucher.PaymentModeId);
                }
            }
            //accounts entry(cash/back account)
            await _accountService.HitAccount(_unitOfWork, paymentVoucher.CostCenterId, paymentVoucher.VoucherNo, paymentVoucher.Remark, paymentVoucher.CashBankAccountId,
             0, paymentVoucher.TotalAmount, paymentVoucher.VoucherDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

        }

        private async Task DeleteTransaction(string code)
        {
            var voucherEntry = await _unitOfWork.Repository<PaymentVoucher>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.VoucherNo == code);
            if (voucherEntry is null) throw new NotFoundResultException("Payment Voucher Not Found With this voucher No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == voucherEntry.VoucherNo);
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
