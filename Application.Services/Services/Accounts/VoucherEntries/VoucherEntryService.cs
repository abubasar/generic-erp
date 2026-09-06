using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.VoucherEntry;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services.Services.Accounts.VoucherEntries
{
    public class VoucherEntryService : BaseService<VoucherEntry, VoucherEntryCreationDto, VoucherEntryUpdateDto, VoucherEntryRequestModel, VoucherEntryViewModel>, IVoucherEntryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ISmsService _smsService;

        public VoucherEntryService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
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
        public async Task<VoucherEntryViewModel> GetByIdAsync(Guid id)
        {
            var voucherEntry = await _unitOfWork.Repository<VoucherEntry>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.CashBankAccount).Include(x => x.PaymentMode).Include(blog => blog.VoucherEntryDetails).ThenInclude(x => x.Account));
            if (voucherEntry == null) throw new NotFoundResultException("Voucher Entry Not Found With this Id");
            return _mapper.Map<VoucherEntryViewModel>(voucherEntry);
        }
        public new async Task<AddUpdateResponseModel> AddAsync(VoucherEntryCreationDto voucherEntryCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            voucherEntryCreationDto.VoucherDate = voucherEntryCreationDto.VoucherDate.ToLocal();
            var voucherEntry = _mapper.Map<VoucherEntry>(voucherEntryCreationDto);
            var vnumber = "";
            var queryable = _unitOfWork.Repository<VoucherEntry>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters();
            if (voucherEntryCreationDto.VoucherType == 0) throw new BadRequestException("Please Select Voucher Type");
            if (voucherEntryCreationDto.VoucherType == (int)VoucherType.Cash_Payment_Voucher)
            {
                var debitVoucherCount = queryable.Where(x => x.VoucherType == (int)VoucherType.Cash_Payment_Voucher).Count() + 1;
                vnumber = "CPV" + financialYear.Code + "-" + debitVoucherCount.ToString().PadLeft(7, '0');
            }
            else
            {
                var creditVoucherCount = queryable.Where(x => x.VoucherType == (int)VoucherType.Cash_Received_Voucher).Count() + 1;
                vnumber = "CRV" + financialYear.Code + "-" + creditVoucherCount.ToString().PadLeft(7, '0');
            }

            voucherEntry.Id = Guid.NewGuid();
            voucherEntry.VoucherNo = vnumber;
            voucherEntry.FinancialYearId = financialYear.Id;
            voucherEntry.Status = (int)VoucherEntryStatus.Pending;
            foreach (var item in voucherEntry.VoucherEntryDetails)
            {
                item.Id = Guid.NewGuid();
                item.VoucherEntryId = voucherEntry.Id;
                await _unitOfWork.Repository<VoucherEntryDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<VoucherEntry>().AddAsync(voucherEntry);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, voucherEntry.Id
                , "/accounts/voucher-entry/" + voucherEntry.Id, Permissions.VoucherEntries.Check,
                "Voucher Entry " + voucherEntry.VoucherNo + " is ready for Check", (int)VoucherEntryStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return new AddUpdateResponseModel { Id = voucherEntry.Id, Code = voucherEntry.VoucherNo, Status = voucherEntry.Status };
        }
        public new async Task<AddUpdateResponseModel> UpdateAsync(VoucherEntryUpdateDto voucherEntryUpdateDto)
        {
            var dbVoucherEntry = await _unitOfWork.Repository<VoucherEntry>().FindAsync(voucherEntryUpdateDto.Id);
            //when date updated,convert it to local date
            if (voucherEntryUpdateDto.VoucherDate.Equals(dbVoucherEntry.VoucherDate) == false)
            {
                voucherEntryUpdateDto.VoucherDate = voucherEntryUpdateDto.VoucherDate.ToLocal();
            }
            var voucherEntry = _mapper.Map(voucherEntryUpdateDto, dbVoucherEntry);
            if (dbVoucherEntry.Status == (int)VoucherEntryStatus.Pending || dbVoucherEntry.Status == (int)VoucherEntryStatus.Checked)
            {
                await _unitOfWork.Repository<VoucherEntry>().UpdateAsync(voucherEntry);
                foreach (var item in voucherEntry.VoucherEntryDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.VoucherEntryId = voucherEntry.Id;
                        await _unitOfWork.Repository<VoucherEntryDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<VoucherEntryDetail>().UpdateAsync(item);
                }
                //Delete for VoucherEntryDetails
                if (!string.IsNullOrEmpty(voucherEntryUpdateDto.DeletedVoucherEntryDetailIds))
                {
                    foreach (var id in voucherEntryUpdateDto.DeletedVoucherEntryDetailIds.Split(',').Where(x => x != ""))
                    {
                        var voucherEntryDetail = await _unitOfWork.Repository<VoucherEntryDetail>().FindAsync(new Guid(id));
                        voucherEntryDetail.Deleted = true;
                        await _unitOfWork.Repository<VoucherEntryDetail>().UpdateAsync(voucherEntryDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return new AddUpdateResponseModel { Id = voucherEntry.Id, Code = voucherEntry.VoucherNo, Status = voucherEntry.Status };
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var voucherEntry = await _unitOfWork.Repository<VoucherEntry>().FindAsync(x => x.Id == id, x => x.Include(x => x.VoucherEntryDetails));
            if (voucherEntry == null) throw new NotFoundResultException("VoucherEntry Not Found With this Id");
            if (voucherEntry.Status == (int)VoucherEntryStatus.Pending)
            {
                foreach (var item in voucherEntry.VoucherEntryDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<VoucherEntryDetail>().UpdateAsync(item);
                }
                voucherEntry.Deleted = true;
                await _unitOfWork.Repository<VoucherEntry>().UpdateAsync(voucherEntry);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return voucherEntry.Id;
        }
        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var voucherEntries = _unitOfWork.Repository<VoucherEntry>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await voucherEntries.Where(x => x.Status == (int)VoucherEntryStatus.Pending).CountAsync(),
                CheckedCount = await voucherEntries.Where(x => x.Status == (int)VoucherEntryStatus.Checked).CountAsync()
            };
            return response;
        }
        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbVoucherEntry = await _unitOfWork.Repository<VoucherEntry>().FindAsync(id);
            if (dbVoucherEntry.Status == (int)VoucherEntryStatus.Pending)
            {
                dbVoucherEntry.Status = (int)VoucherEntryStatus.Checked;
                dbVoucherEntry.CheckedBy = _workContext.GetUserName();
                //update VoucherEntry
                await _unitOfWork.Repository<VoucherEntry>().UpdateAsync(dbVoucherEntry);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbVoucherEntry.Id
                       && x.Status == (int)VoucherEntryStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbVoucherEntry.Id
                    , "/accounts/voucher-entry/" + dbVoucherEntry.Id, Permissions.VoucherEntries.Approve,
                    "Voucher Entry " + dbVoucherEntry.VoucherNo + " is ready for Approval", (int)VoucherEntryStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Voucher Entry Status has already been checked by " + dbVoucherEntry?.CheckedBy);
        }
        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<VoucherEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Voucher Entry not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Voucher Entry does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<VoucherEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)VoucherEntryStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)VoucherEntryStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Voucher Entry already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbVoucherEntry = await _unitOfWork.Repository<VoucherEntry>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.VoucherEntryDetails).ThenInclude(x => x.Account));

            dbVoucherEntry.Status = (int)VoucherEntryStatus.Approved;
            dbVoucherEntry.ApprovedBy = approvedBy;

            await InsertTransaction(dbVoucherEntry);

            //update VoucherEntry
            await _unitOfWork.Repository<VoucherEntry>().UpdateAsync(dbVoucherEntry);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbVoucherEntry.Id
                   && x.Status == (int)VoucherEntryStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            //send sms
            if (dbVoucherEntry.VoucherType == (int)VoucherType.Cash_Received_Voucher)
            {
                foreach (var account in dbVoucherEntry.VoucherEntryDetails)
                {
                    if (account?.Account.ParentId == AccountHeadConstants.TradeReceivable.ToGuid() && !string.IsNullOrWhiteSpace(account?.Account?.ContactNo))
                    {
                        var meassage = $"Thanks For Deposit Money for Feed Purpose.{Environment.NewLine}" +
                    $"Details:https://invoice.butsbd.com/mr/{dbVoucherEntry.VoucherNo}";
                        //await _smsService.Send(account.Account.ContactNo, meassage);
                    }
                }
            }
            return isApproved;
        }
        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<VoucherEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("VoucherEntry Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Voucher Entry does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)VoucherEntryStatus.Checked && dbInfo.Status != (int)VoucherEntryStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)VoucherEntryStatus.Checked
                ? (int)VoucherEntryStatus.Pending
                : (int)VoucherEntryStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<VoucherEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbVoucherEntry = await _unitOfWork.Repository<VoucherEntry>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.VoucherEntryDetails));
            dbVoucherEntry.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)VoucherEntryStatus.Checked:
                    dbVoucherEntry.CheckedBy = "";
                    break;
                case (int)VoucherEntryStatus.Approved:
                    dbVoucherEntry.ApprovedBy = "";
                    await DeleteTransaction(dbVoucherEntry.VoucherNo);
                    break;
            }

            await _unitOfWork.Repository<VoucherEntry>().UpdateAsync(dbVoucherEntry);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbVoucherEntry.Status;
        }

        private async Task InsertTransaction(VoucherEntry voucherEntry)
        {
            if (voucherEntry.VoucherEntryDetails.Sum(x => x.Amount) != voucherEntry.TotalAmount) throw new BadRequestException("Debit Credit Not Equal!");
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var cashBankAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == voucherEntry.CashBankAccountId).Select(x => x.Name).FirstOrDefaultAsync();
            if (voucherEntry.VoucherType == (int)VoucherType.Cash_Received_Voucher)
            {
                contraAccountNamesForCredit.Append(cashBankAccountName);
                contraAccountIdsForCredit.Append(voucherEntry.CashBankAccountId.ToString());
                var creditCounter = 0M;
                foreach (var item in voucherEntry.VoucherEntryDetails)
                {
                    var accountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.AccountId).Select(x => x.Name).FirstOrDefaultAsync();
                    if (creditCounter > 0)
                    {
                        contraAccountNamesForDebit.Append(", ");
                        contraAccountIdsForDebit.Append(", ");
                    }
                    contraAccountNamesForDebit.Append(accountName);
                    contraAccountIdsForDebit.Append(item.AccountId.ToString());
                    creditCounter++;
                }
            }
            if (voucherEntry.VoucherType == (int)VoucherType.Cash_Payment_Voucher)
            {
                contraAccountNamesForDebit.Append(cashBankAccountName);
                contraAccountIdsForDebit.Append(voucherEntry.CashBankAccountId.ToString());
                var debitCounter = 0M;
                foreach (var item in voucherEntry.VoucherEntryDetails)
                {
                    var accountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.AccountId).Select(x => x.Name).FirstOrDefaultAsync();
                    if (debitCounter > 0)
                    {
                        contraAccountNamesForCredit.Append(", ");
                        contraAccountIdsForCredit.Append(", ");
                    }
                    contraAccountNamesForCredit.Append(accountName);
                    contraAccountIdsForCredit.Append(item.AccountId.ToString());
                    debitCounter++;
                }
            }
            //Transaction entries start here
            foreach (var item in voucherEntry.VoucherEntryDetails)
            {
                if (voucherEntry.VoucherType == (int)VoucherType.Cash_Received_Voucher)
                {
                    if (item?.Account.ParentId == AccountHeadConstants.TradeReceivable.ToGuid())
                    {
                        await _accountService.HitAccount(_unitOfWork, voucherEntry.CostCenterId, voucherEntry.VoucherNo, "Voucher Entry " + voucherEntry.Remark,
                         item!.AccountId, 0, item.Amount, voucherEntry.VoucherDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.CustomerReceipt, 0, 0m, voucherEntry.PaymentModeId);
                    }
                    else
                    {
                        await _accountService.HitAccount(_unitOfWork, voucherEntry.CostCenterId, voucherEntry.VoucherNo, "Voucher Entry " + voucherEntry.Remark,
                         item!.AccountId, 0, item.Amount, voucherEntry.VoucherDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.OtherReceipt, 0, 0m, voucherEntry.PaymentModeId);
                    }
                }
                if (voucherEntry.VoucherType == (int)VoucherType.Cash_Payment_Voucher)
                {
                    if (item?.Account.ParentId == AccountHeadConstants.TradePayable.ToGuid())
                    {
                        await _accountService.HitAccount(_unitOfWork, voucherEntry.CostCenterId, voucherEntry.VoucherNo, "Voucher Entry " + voucherEntry.Remark,
                         item!.AccountId, item.Amount, 0, voucherEntry.VoucherDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.SupplierPayment, 0, 0m, voucherEntry.PaymentModeId);
                    }
                    else
                    {
                        await _accountService.HitAccount(_unitOfWork, voucherEntry.CostCenterId, voucherEntry.VoucherNo, "Voucher Entry " + voucherEntry.Remark,
                         item!.AccountId, item.Amount, 0, voucherEntry.VoucherDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.OtherPayment, 0, 0m, voucherEntry.PaymentModeId);
                    }
                }

            }
            //accounts entry(cash/back account)
            await _accountService.HitAccount(_unitOfWork, voucherEntry.CostCenterId, voucherEntry.VoucherNo, "Voucher Entry " + voucherEntry.Remark, voucherEntry.CashBankAccountId,
            voucherEntry.VoucherType == (int)VoucherType.Cash_Payment_Voucher ? 0 : voucherEntry.TotalAmount,
            voucherEntry.VoucherType == (int)VoucherType.Cash_Received_Voucher ? 0 : voucherEntry.TotalAmount, voucherEntry.VoucherDate, voucherEntry.VoucherType == (int)VoucherType.Cash_Payment_Voucher ? contraAccountIdsForCredit.ToString() : contraAccountIdsForDebit.ToString(),
            voucherEntry.VoucherType == (int)VoucherType.Cash_Payment_Voucher ? contraAccountNamesForCredit.ToString() : contraAccountNamesForDebit.ToString());

        }

        private async Task DeleteTransaction(string code)
        {
            var voucherEntry = await _unitOfWork.Repository<VoucherEntry>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.VoucherNo == code);
            if (voucherEntry is null) throw new NotFoundResultException("Voucher Entry Not Found With this voucher No");
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
