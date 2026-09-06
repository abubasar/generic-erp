using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.PaymentVoucher;
using Application.Services.Dtos.Accounts.ReceiveVoucher;
using Application.Services.Dtos.Purchase.SupplierPayment;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.ReceiveVoucher;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services.Services.Accounts.ReceiveVouchers
{
    public class ReceiveVoucherService : BaseService<ReceiveVoucher, ReceiveVoucherCreationDto, ReceiveVoucherUpdateDto, ReceiveVoucherRequestModel, ReceiveVoucherViewModel>, IReceiveVoucherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ISmsService _smsService;

        public ReceiveVoucherService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
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

        public async Task<ReceiveVoucherAggregatorModel> PrepareReceiveVoucherAggregatorModel(ReceiveVoucherRequestModel request)
        {
            var receiveVoucherQueryable = _unitOfWork.Repository<ReceiveVoucher>().TableNoTracking().Where(request.GetExpression());
            return new ReceiveVoucherAggregatorModel
            {
                AggregatorTotalAmount = await receiveVoucherQueryable.SumAsync(x => x.TotalAmount)
            };
        }

        public async Task<ReceiveVoucherViewModel> GetByIdAsync(Guid id)
        {
            var receiveVoucher = await _unitOfWork.Repository<ReceiveVoucher>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.CashBankAccount).Include(x => x.PaymentMode).Include(blog => blog.ReceiveVoucherDetails).ThenInclude(x => x.Account));
            if (receiveVoucher == null) throw new NotFoundResultException("Receive Voucher Not Found With this Id");
            return _mapper.Map<ReceiveVoucherViewModel>(receiveVoucher);
        }

        public new async Task<Guid> AddAsync(ReceiveVoucherCreationDto receiveVoucherCreationDto)
        {
            if (!string.IsNullOrEmpty(receiveVoucherCreationDto.TransactionNumber))
            {
                var existingTransaction = await _unitOfWork.Repository<ReceiveVoucher>().TableNoTracking().AnyAsync(x => x.TransactionNumber == receiveVoucherCreationDto.TransactionNumber);
                if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
            }
            var financialYear = _workContext.GetCurrentFinancialYear();
            receiveVoucherCreationDto.VoucherDate = receiveVoucherCreationDto.VoucherDate.ToLocal();
            var receiveVoucher = _mapper.Map<ReceiveVoucher>(receiveVoucherCreationDto);
            var queryable = _unitOfWork.Repository<ReceiveVoucher>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters();
            var costCenter = await _unitOfWork.Repository<CostCenter>().FindAsync(receiveVoucherCreationDto.CostCenterId);
            if (costCenter is null) throw new BadRequestException("Please Select Cost Center");
            var receiveVoucherCount = queryable.Where(x => x.CostCenterId == costCenter.Id).Count() + 1;
            receiveVoucher.VoucherNo = "CRV" + costCenter.Name.Substring(0, 1) + financialYear.Code + "-" + receiveVoucherCount.ToString().PadLeft(7, '0');
            receiveVoucher.Id = Guid.NewGuid();
            receiveVoucher.FinancialYearId = financialYear.Id;
            receiveVoucher.Status = (int)ReceiveVoucherStatus.Pending;
            foreach (var item in receiveVoucher.ReceiveVoucherDetails)
            {
                item.Id = Guid.NewGuid();
                item.ReceiveVoucherId = receiveVoucher.Id;
                await _unitOfWork.Repository<ReceiveVoucherDetail>().AddAsync(item);
            }

            await _unitOfWork.Repository<ReceiveVoucher>().AddAsync(receiveVoucher);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, receiveVoucher.Id
                , "/accounts/receive-voucher/" + receiveVoucher.Id, Permissions.ReceiveVouchers.Check,
                "Receive Voucher " + receiveVoucher.VoucherNo + " is ready for Check", (int)ReceiveVoucherStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return receiveVoucher.Id;
        }

        public new async Task<Guid> UpdateAsync(ReceiveVoucherUpdateDto receiveVoucherUpdateDto)
        {
            var dbReceiveVoucher = await _unitOfWork.Repository<ReceiveVoucher>().FindAsync(receiveVoucherUpdateDto.Id);
            if (!string.IsNullOrEmpty(receiveVoucherUpdateDto.TransactionNumber))
            {
                if (receiveVoucherUpdateDto.TransactionNumber.Equals(dbReceiveVoucher.TransactionNumber) == false)
                {
                    var existingTransaction = await _unitOfWork.Repository<ReceiveVoucher>().TableNoTracking().AnyAsync(x => x.TransactionNumber == receiveVoucherUpdateDto.TransactionNumber);
                    if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
                }
            }
            //when date updated,convert it to local date
            if (receiveVoucherUpdateDto.VoucherDate.Equals(dbReceiveVoucher.VoucherDate) == false)
            {
                receiveVoucherUpdateDto.VoucherDate = receiveVoucherUpdateDto.VoucherDate.ToLocal();
            }
            var receiveVoucher = _mapper.Map(receiveVoucherUpdateDto, dbReceiveVoucher);
            if (dbReceiveVoucher.Status == (int)ReceiveVoucherStatus.Pending || dbReceiveVoucher.Status == (int)ReceiveVoucherStatus.Checked)
            {
                await _unitOfWork.Repository<ReceiveVoucher>().UpdateAsync(receiveVoucher);
                foreach (var item in receiveVoucher.ReceiveVoucherDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.ReceiveVoucherId = receiveVoucher.Id;
                        await _unitOfWork.Repository<ReceiveVoucherDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<ReceiveVoucherDetail>().UpdateAsync(item);
                }
                //Delete for ReceiveVoucherDetails
                if (!string.IsNullOrEmpty(receiveVoucherUpdateDto.DeletedReceiveVoucherDetailIds))
                {
                    foreach (var id in receiveVoucherUpdateDto.DeletedReceiveVoucherDetailIds.Split(',').Where(x => x != ""))
                    {
                        var receiveVoucherDetail = await _unitOfWork.Repository<ReceiveVoucherDetail>().FindAsync(new Guid(id));
                        receiveVoucherDetail.Deleted = true;
                        await _unitOfWork.Repository<ReceiveVoucherDetail>().UpdateAsync(receiveVoucherDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return receiveVoucher.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var receiveVoucher = await _unitOfWork.Repository<ReceiveVoucher>().FindAsync(x => x.Id == id, x => x.Include(x => x.ReceiveVoucherDetails));
            if (receiveVoucher == null) throw new NotFoundResultException("ReceiveVoucher Not Found With this Id");
            if (receiveVoucher.Status == (int)ReceiveVoucherStatus.Pending)
            {
                foreach (var item in receiveVoucher.ReceiveVoucherDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<ReceiveVoucherDetail>().UpdateAsync(item);
                }
                receiveVoucher.Deleted = true;
                await _unitOfWork.Repository<ReceiveVoucher>().UpdateAsync(receiveVoucher);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return receiveVoucher.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var receiveVouchers = _unitOfWork.Repository<ReceiveVoucher>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await receiveVouchers.Where(x => x.Status == (int)ReceiveVoucherStatus.Pending).CountAsync(),
                CheckedCount = await receiveVouchers.Where(x => x.Status == (int)ReceiveVoucherStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbReceiveVoucher = await _unitOfWork.Repository<ReceiveVoucher>().FindAsync(id);
            if (dbReceiveVoucher.Status == (int)ReceiveVoucherStatus.Pending)
            {
                dbReceiveVoucher.Status = (int)ReceiveVoucherStatus.Checked;
                dbReceiveVoucher.CheckedBy = _workContext.GetUserName();
                //update ReceiveVoucher
                await _unitOfWork.Repository<ReceiveVoucher>().UpdateAsync(dbReceiveVoucher);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbReceiveVoucher.Id
                       && x.Status == (int)ReceiveVoucherStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbReceiveVoucher.Id
                    , "/accounts/receive-voucher/" + dbReceiveVoucher.Id, Permissions.ReceiveVouchers.Approve,
                    "Receive Voucher " + dbReceiveVoucher.VoucherNo + " is ready for Approval", (int)ReceiveVoucherStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Receive Voucher Status has already been checked by " + dbReceiveVoucher?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<ReceiveVoucher>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Receive Voucher not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Receive Voucher does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<ReceiveVoucher>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)ReceiveVoucherStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)ReceiveVoucherStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Receive Voucher already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbReceiveVoucher = await _unitOfWork.Repository<ReceiveVoucher>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.ReceiveVoucherDetails).ThenInclude(x => x.Account));

            dbReceiveVoucher.Status = (int)ReceiveVoucherStatus.Approved;
            dbReceiveVoucher.ApprovedBy = approvedBy;

            await InsertTransaction(dbReceiveVoucher);
            //update ReceiveVoucher
            await _unitOfWork.Repository<ReceiveVoucher>().UpdateAsync(dbReceiveVoucher);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbReceiveVoucher.Id
                   && x.Status == (int)ReceiveVoucherStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<ReceiveVoucher>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("ReceiveVoucher Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Receive Voucher does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)ReceiveVoucherStatus.Checked && dbInfo.Status != (int)ReceiveVoucherStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)ReceiveVoucherStatus.Checked
                ? (int)ReceiveVoucherStatus.Pending
                : (int)ReceiveVoucherStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<ReceiveVoucher>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbReceiveVoucher = await _unitOfWork.Repository<ReceiveVoucher>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.ReceiveVoucherDetails));
            dbReceiveVoucher.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)ReceiveVoucherStatus.Checked:
                    dbReceiveVoucher.CheckedBy = "";
                    break;
                case (int)ReceiveVoucherStatus.Approved:
                    dbReceiveVoucher.ApprovedBy = "";
                    await DeleteTransaction(dbReceiveVoucher.VoucherNo);
                    break;
            }

            await _unitOfWork.Repository<ReceiveVoucher>().UpdateAsync(dbReceiveVoucher);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbReceiveVoucher.Status;
        }

        private async Task InsertTransaction(ReceiveVoucher receiveVoucher)
        {
            if (receiveVoucher.ReceiveVoucherDetails.Sum(x => x.Amount) != receiveVoucher.TotalAmount) throw new BadRequestException("Debit Credit Not Equal!");
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var cashBankAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == receiveVoucher.CashBankAccountId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(cashBankAccountName);
            contraAccountIdsForCredit.Append(receiveVoucher.CashBankAccountId.ToString());
            var counter = 0M;
            foreach (var item in receiveVoucher.ReceiveVoucherDetails)
            {
                var accountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.AccountId).Select(x => x.Name).FirstOrDefaultAsync();
                if (counter > 0)
                {
                    contraAccountNamesForDebit.Append(", ");
                    contraAccountIdsForDebit.Append(", ");
                }
                contraAccountNamesForDebit.Append(accountName);
                contraAccountIdsForDebit.Append(item.AccountId.ToString());
                counter++;
            }
            //Transaction entries start here
            foreach (var item in receiveVoucher.ReceiveVoucherDetails)
            {

                if (item?.Account.ParentId == AccountHeadConstants.TradeReceivable.ToGuid())
                {
                    await _accountService.HitAccount(_unitOfWork, receiveVoucher.CostCenterId, receiveVoucher.VoucherNo, receiveVoucher.Remark,
                     item!.AccountId, 0, item.Amount, receiveVoucher.VoucherDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.CustomerReceipt, 0, 0m, receiveVoucher.PaymentModeId);
                }
                else
                {
                    await _accountService.HitAccount(_unitOfWork, receiveVoucher.CostCenterId, receiveVoucher.VoucherNo, receiveVoucher.Remark,
                     item!.AccountId, 0, item.Amount, receiveVoucher.VoucherDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.OtherReceipt, 0, 0m, receiveVoucher.PaymentModeId);
                }


            }
            //accounts entry(cash/back account)
            await _accountService.HitAccount(_unitOfWork, receiveVoucher.CostCenterId, receiveVoucher.VoucherNo, receiveVoucher.Remark, receiveVoucher.CashBankAccountId,
            receiveVoucher.TotalAmount, 0, receiveVoucher.VoucherDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

        }

        private async Task DeleteTransaction(string code)
        {
            var voucherEntry = await _unitOfWork.Repository<ReceiveVoucher>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.VoucherNo == code);
            if (voucherEntry is null) throw new NotFoundResultException("Receive Voucher Not Found With this voucher No");
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
