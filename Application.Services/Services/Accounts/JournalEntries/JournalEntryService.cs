using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.JournalEntry;
using Application.Services.Dtos.Accounts.PaymentVoucher;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Accounts.JournalEntries
{
    public class JournalEntryService : BaseService<JournalEntry, JournalEntryCreationDto, JournalEntryUpdateDto, JournalEntryRequestModel, JournalEntryViewModel>, IJournalEntryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;

        public JournalEntryService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
            IHubContext<BroadcastHub, IHubClient> hubContext, IAccountService accountService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
        }

        public async Task<JournalEntryViewModel> GetByIdAsync(Guid id)
        {
            var journalEntry = await _unitOfWork.Repository<JournalEntry>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.JournalEntryDetails).ThenInclude(x => x.Account));
            if (journalEntry == null) throw new NotFoundResultException("Journal Entry Not Found With this Id");
            return _mapper.Map<JournalEntryViewModel>(journalEntry);
        }
        public new async Task<Guid> AddAsync(JournalEntryCreationDto journalEntryCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            journalEntryCreationDto.VoucherDate = journalEntryCreationDto.VoucherDate.ToLocal();
            var journalEntry = _mapper.Map<JournalEntry>(journalEntryCreationDto);
            var queryable = _unitOfWork.Repository<JournalEntry>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters();
            var costCenter = await _unitOfWork.Repository<CostCenter>().FindAsync(journalEntryCreationDto.CostCenterId);
            if (costCenter is null) throw new BadRequestException("Please Select Cost Center");
            var journalVoucherCount = queryable.Where(x => x.CostCenterId == costCenter.Id).Count() + 1;
            journalEntry.VoucherNo = "JV" + costCenter.Name.Substring(0, 1) + financialYear.Code + "-" + journalVoucherCount.ToString().PadLeft(7, '0');
            journalEntry.Id = Guid.NewGuid();
            journalEntry.FinancialYearId = financialYear.Id;
            journalEntry.Status = (int)JournalEntryStatus.Pending;
            foreach (var item in journalEntry.JournalEntryDetails)
            {
                item.Id = Guid.NewGuid();
                item.JournalEntryId = journalEntry.Id;
                await _unitOfWork.Repository<JournalEntryDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<JournalEntry>().AddAsync(journalEntry);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, journalEntry.Id
                , "/accounts/journal-entry/" + journalEntry.Id, Permissions.JournalEntries.Check,
                "Journal Entry " + journalEntry.VoucherNo + " is ready for Check", (int)JournalEntryStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return journalEntry.Id;
        }
        public new async Task<Guid> UpdateAsync(JournalEntryUpdateDto journalEntryUpdateDto)
        {
            var dbJournalEntry = await _unitOfWork.Repository<JournalEntry>().FindAsync(journalEntryUpdateDto.Id);
            //when date updated,convert it to local date
            if (journalEntryUpdateDto.VoucherDate.Equals(dbJournalEntry.VoucherDate) == false)
            {
                journalEntryUpdateDto.VoucherDate = journalEntryUpdateDto.VoucherDate.ToLocal();
            }
            var journalEntry = _mapper.Map(journalEntryUpdateDto, dbJournalEntry);
            if (dbJournalEntry.Status == (int)JournalEntryStatus.Pending || dbJournalEntry.Status == (int)JournalEntryStatus.Checked)
            {
                await _unitOfWork.Repository<JournalEntry>().UpdateAsync(journalEntry);
                foreach (var item in journalEntry.JournalEntryDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.JournalEntryId = journalEntry.Id;
                        await _unitOfWork.Repository<JournalEntryDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<JournalEntryDetail>().UpdateAsync(item);
                }
                //Delete for JournalEntryDetails
                if (!string.IsNullOrEmpty(journalEntryUpdateDto.DeletedJournalEntryDetailIds))
                {
                    foreach (var id in journalEntryUpdateDto.DeletedJournalEntryDetailIds.Split(',').Where(x => x != ""))
                    {
                        var journalEntryDetail = await _unitOfWork.Repository<JournalEntryDetail>().FindAsync(new Guid(id));
                        journalEntryDetail.Deleted = true;
                        await _unitOfWork.Repository<JournalEntryDetail>().UpdateAsync(journalEntryDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return journalEntry.Id;
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var journalEntry = await _unitOfWork.Repository<JournalEntry>().FindAsync(x => x.Id == id, x => x.Include(x => x.JournalEntryDetails));
            if (journalEntry == null) throw new NotFoundResultException("JournalEntry Not Found With this Id");
            if (journalEntry.Status == (int)JournalEntryStatus.Pending)
            {
                foreach (var item in journalEntry.JournalEntryDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<JournalEntryDetail>().UpdateAsync(item);
                }
                journalEntry.Deleted = true;
                await _unitOfWork.Repository<JournalEntry>().UpdateAsync(journalEntry);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return journalEntry.Id;
        }
        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var journalEntries = _unitOfWork.Repository<JournalEntry>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await journalEntries.Where(x => x.Status == (int)JournalEntryStatus.Pending).CountAsync(),
                CheckedCount = await journalEntries.Where(x => x.Status == (int)JournalEntryStatus.Checked).CountAsync()
            };
            return response;
        }
        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbJournalEntry = await _unitOfWork.Repository<JournalEntry>().FindAsync(id);
            if (dbJournalEntry.Status == (int)JournalEntryStatus.Pending)
            {
                dbJournalEntry.Status = (int)JournalEntryStatus.Checked;
                dbJournalEntry.CheckedBy = _workContext.GetUserName();
                //update JournalEntry
                await _unitOfWork.Repository<JournalEntry>().UpdateAsync(dbJournalEntry);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbJournalEntry.Id
                       && x.Status == (int)JournalEntryStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbJournalEntry.Id
                    , "/accounts/journal-entry/" + dbJournalEntry.Id, Permissions.JournalEntries.Approve,
                    "Journal Entry " + dbJournalEntry.VoucherNo + " is ready for Approval", (int)JournalEntryStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Journal Entry Status has already been checked by " + dbJournalEntry?.CheckedBy);
        }
        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<JournalEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Journal Entry not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Journal Entry does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<JournalEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)JournalEntryStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)JournalEntryStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Journal Entry already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbJournalEntry = await _unitOfWork.Repository<JournalEntry>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.JournalEntryDetails));

            dbJournalEntry.Status = (int)JournalEntryStatus.Approved;
            dbJournalEntry.ApprovedBy = approvedBy;

            await InsertTransaction(dbJournalEntry);
            //update JournalEntry
            await _unitOfWork.Repository<JournalEntry>().UpdateAsync(dbJournalEntry);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbJournalEntry.Id
                   && x.Status == (int)JournalEntryStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<JournalEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("JournalEntry Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Journal Entry does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)JournalEntryStatus.Checked && dbInfo.Status != (int)JournalEntryStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)JournalEntryStatus.Checked
                ? (int)JournalEntryStatus.Pending
                : (int)JournalEntryStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<JournalEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbJournalEntry = await _unitOfWork.Repository<JournalEntry>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.JournalEntryDetails));
            dbJournalEntry.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)JournalEntryStatus.Checked:
                    dbJournalEntry.CheckedBy = "";
                    break;
                case (int)JournalEntryStatus.Approved:
                    dbJournalEntry.ApprovedBy = "";
                    await DeleteTransaction(dbJournalEntry.VoucherNo);
                    break;
            }

            await _unitOfWork.Repository<JournalEntry>().UpdateAsync(dbJournalEntry);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbJournalEntry.Status;
        }
        private async Task InsertTransaction(JournalEntry journalEntry)
        {
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var counterCredit = 0M;
            var counterDebit = 0M;
            foreach (var item in journalEntry.JournalEntryDetails)
            {
                var accountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.AccountId).Select(x => x.Name).FirstOrDefaultAsync();
                if (item.PostType == 1)
                {
                    if (counterDebit > 0)
                    {
                        contraAccountNamesForCredit.Append(", ");
                        contraAccountIdsForCredit.Append(", ");
                    }
                    contraAccountNamesForCredit.Append(accountName);
                    contraAccountIdsForCredit.Append(item.AccountId.ToString());
                    counterDebit++;
                }
                if (item.PostType == 2)
                {
                    if (counterCredit > 0)
                    {
                        contraAccountNamesForDebit.Append(", ");
                        contraAccountIdsForDebit.Append(", ");
                    }
                    contraAccountNamesForDebit.Append(accountName);
                    contraAccountIdsForDebit.Append(item.AccountId.ToString());
                    counterCredit++;
                }
            }
            if (journalEntry.JournalEntryDetails.Where(x => x.PostType == 1).Sum(x => x.Amount) !=
                journalEntry.JournalEntryDetails.Where(x => x.PostType == 2).Sum(x => x.Amount)) throw new BadRequestException("Debit Credit Not Equal!");
            foreach (var item in journalEntry.JournalEntryDetails)
            {
                await _accountService.HitAccount(_unitOfWork, journalEntry.CostCenterId, journalEntry.VoucherNo, journalEntry.Remark,
                item.AccountId, item.PostType == 1 ? item.Amount : 0, item.PostType == 1 ? 0 : item.Amount, journalEntry.VoucherDate, item.PostType == 1 ? contraAccountIdsForDebit.ToString() : contraAccountIdsForCredit.ToString(),
                item.PostType == 1 ? contraAccountNamesForDebit.ToString() : contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.JournalEntry, 0, 0m, null, item.IsOpeningBalance);
            }

        }

        private async Task DeleteTransaction(string code)
        {
            var journalEntry = await _unitOfWork.Repository<JournalEntry>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.VoucherNo == code);
            if (journalEntry is null) throw new NotFoundResultException("Journal Entry Not Found With this voucher No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == journalEntry.VoucherNo);
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
