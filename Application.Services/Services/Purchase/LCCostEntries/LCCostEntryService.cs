using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.FundTransfer;
using Application.Services.Dtos.Accounts.JournalEntry;
using Application.Services.Dtos.Purchase.LCCostEntry;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.GoodsReceiveNote;
using Application.Services.ViewModels.Purchase.LCCostEntry;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Purchase.LCCostEntries
{
    public class LCCostEntryService : BaseService<LccostEntry, LCCostEntryCreationDto, LCCostEntryUpdateDto, LCCostEntryRequestModel, LCCostEntryViewModel>, ILCCostEntryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;

        public LCCostEntryService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService, IAccountService accountService, IHubContext<BroadcastHub, IHubClient> hubContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _accountService = accountService;
            _hubContext = hubContext;
        }

        public async Task<LCCostEntryAggregatorModel> PrepareLCCostEntryAggregatorModel(LCCostEntryRequestModel request)
        {
            var lCCostEntriesQueryable = _unitOfWork.Repository<LccostEntry>().TableNoTracking().Where(request.GetExpression());
            return new LCCostEntryAggregatorModel
            {
                AggregatorTotal = await lCCostEntriesQueryable.SumAsync(x => x.Total),
            };
        }

        public async Task<LCCostEntryViewModel> GetByIdAsync(Guid id)
        {
            var lCCostEntry = await _unitOfWork.Repository<LccostEntry>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.PurchaseOrder).Include(x => x.LccostEntryDetails).ThenInclude(x => x.DebitAccount).Include(x => x.LccostEntryDetails).ThenInclude(x => x.CreditAccount));
            if (lCCostEntry == null) throw new NotFoundResultException("LC Cost Entry Not Found With this Id");
            return _mapper.Map<LCCostEntryViewModel>(lCCostEntry);
        }

        public async Task<List<LcCostEntryDetailAgainstPoViewModel>> GetLcCostEntriesByPoAsync(Guid purchaseOrderId)
        {
            var lcCostEntries = await _unitOfWork.Repository<LccostEntry>().TableNoTracking().Where(x => x.PurchaseOrderId == purchaseOrderId).Include(x => x.LccostEntryDetails).ThenInclude(x => x.DebitAccount).Include(x => x.LccostEntryDetails).ThenInclude(x => x.CreditAccount).ToListAsync();
            if (lcCostEntries == null) throw new NotFoundResultException("LC Cost Entry Not Found With this Purchase Order");
            List<LcCostEntryDetailAgainstPoViewModel> lcCostEntryDetails = new();
            foreach (var lcCostEntry in lcCostEntries)
            {
                foreach (var item in lcCostEntry.LccostEntryDetails)
                {
                    lcCostEntryDetails.Add(new LcCostEntryDetailAgainstPoViewModel()
                    {
                        LcCostEntryNo = lcCostEntry.Code,
                        EntryDate = lcCostEntry.EntryDate,
                        DebitAccountName = item.DebitAccount.Name,
                        CreditAccountName = item.CreditAccount.Name,
                        Amount = item.Amount,
                        IsIncludedWithinLandedCost = item.IsIncludedWithinLandedCost,
                        Status = lcCostEntry.Status
                    });
                }
                ;
            }
            return lcCostEntryDetails.OrderBy(x => x.LcCostEntryNo).ToList();
        }

        public new async Task<Guid> AddAsync(LCCostEntryCreationDto lCCostEntryCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            lCCostEntryCreationDto.EntryDate = lCCostEntryCreationDto.EntryDate.ToLocal();
            var lCCostEntry = _mapper.Map<LccostEntry>(lCCostEntryCreationDto);
            var count = _unitOfWork.Repository<LccostEntry>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            lCCostEntry.Id = Guid.NewGuid();
            lCCostEntry.Code = "LCC" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            lCCostEntry.FinancialYearId = financialYear.Id;
            lCCostEntry.Status = (int)LCCostEntryStatus.Pending;
            foreach (var item in lCCostEntry.LccostEntryDetails)
            {
                item.Id = Guid.NewGuid();
                item.LccostEntryId = lCCostEntry.Id;
                await _unitOfWork.Repository<LccostEntryDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<LccostEntry>().AddAsync(lCCostEntry);

            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, lCCostEntry.Id
                , "/purchase/lc-cost-entry/" + lCCostEntry.Id, Permissions.LCCostEntries.Check,
                "LC Cost Entry " + lCCostEntry.Code + " is ready for Check", (int)LCCostEntryStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return lCCostEntry.Id;
        }

        public new async Task<Guid> UpdateAsync(LCCostEntryUpdateDto lCCostEntryUpdateDto)
        {
            var dbLcCostEntry = await _unitOfWork.Repository<LccostEntry>().FindAsync(lCCostEntryUpdateDto.Id);
            //when date updated,convert it to local date
            if (lCCostEntryUpdateDto.EntryDate.Equals(dbLcCostEntry.EntryDate) == false)
            {
                lCCostEntryUpdateDto.EntryDate = lCCostEntryUpdateDto.EntryDate.ToLocal();
            }
            var lCCostEntry = _mapper.Map(lCCostEntryUpdateDto, dbLcCostEntry);
            if (dbLcCostEntry.Status == (int)LCCostEntryStatus.Pending || dbLcCostEntry.Status == (int)LCCostEntryStatus.Checked)
            {
                await _unitOfWork.Repository<LccostEntry>().UpdateAsync(lCCostEntry);
                foreach (var item in lCCostEntry.LccostEntryDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.LccostEntryId = lCCostEntry.Id;
                        await _unitOfWork.Repository<LccostEntryDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<LccostEntryDetail>().UpdateAsync(item);
                }
                //Delete for LccostEntryDetails
                if (!string.IsNullOrEmpty(lCCostEntryUpdateDto.DeletedLCCostEntryDetailIds))
                {
                    foreach (var id in lCCostEntryUpdateDto.DeletedLCCostEntryDetailIds.Split(',').Where(x => x != ""))
                    {
                        var lccostEntryDetail = await _unitOfWork.Repository<LccostEntryDetail>().FindAsync(new Guid(id));
                        lccostEntryDetail.Deleted = true;
                        await _unitOfWork.Repository<LccostEntryDetail>().UpdateAsync(lccostEntryDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return lCCostEntry.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var lCCostEntry = await _unitOfWork.Repository<LccostEntry>().FindAsync(x => x.Id == id, x => x.Include(x => x.LccostEntryDetails));
            if (lCCostEntry == null) throw new NotFoundResultException("LC Cost Entry Not Found With this Id");
            if (lCCostEntry.Status == (int)LCCostEntryStatus.Pending)
            {
                foreach (var item in lCCostEntry.LccostEntryDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<LccostEntryDetail>().UpdateAsync(item);
                }
                lCCostEntry.Deleted = true;
                await _unitOfWork.Repository<LccostEntry>().UpdateAsync(lCCostEntry);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return lCCostEntry.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var lCCostEntries = _unitOfWork.Repository<LccostEntry>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await lCCostEntries.Where(x => x.Status == (int)LCCostEntryStatus.Pending).CountAsync(),
                CheckedCount = await lCCostEntries.Where(x => x.Status == (int)LCCostEntryStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbLcCostEntry = await _unitOfWork.Repository<LccostEntry>().FindAsync(id);
            if (dbLcCostEntry.Status == (int)LCCostEntryStatus.Pending)
            {
                dbLcCostEntry.Status = (int)LCCostEntryStatus.Checked;
                dbLcCostEntry.CheckedBy = _workContext.GetUserName();
                //update LCCostEntry
                await _unitOfWork.Repository<LccostEntry>().UpdateAsync(dbLcCostEntry);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbLcCostEntry.Id
                       && x.Status == (int)LCCostEntryStatus.Pending);

                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbLcCostEntry.Id
                    , "/purchase/lc-cost-entry/" + dbLcCostEntry.Id, Permissions.LCCostEntries.Approve,
                    "LC Cost Entry " + dbLcCostEntry.Code + " is ready for Approval", (int)LCCostEntryStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("LC Cost Entry Status has already been checked by " + dbLcCostEntry?.CheckedBy);
        }

        private async Task InsertTransaction(LccostEntry lcCostEntry)
        {
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var counter = 0M;
            foreach (var item in lcCostEntry.LccostEntryDetails)
            {
                var creditAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.CreditAccountId).Select(x => x.Name).FirstOrDefaultAsync();
                var debitAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.DebitAccountId).Select(x => x.Name).FirstOrDefaultAsync();
                if (counter > 0)
                {
                    contraAccountNamesForCredit.Append(", ");
                    contraAccountIdsForCredit.Append(", ");
                    contraAccountNamesForDebit.Append(", ");
                    contraAccountIdsForDebit.Append(", ");
                }
                contraAccountNamesForCredit.Append(debitAccountName);
                contraAccountIdsForCredit.Append(item.DebitAccountId.ToString());
                contraAccountNamesForDebit.Append(creditAccountName);
                contraAccountIdsForDebit.Append(item.CreditAccountId.ToString());
                counter++;
            }
            //Transaction Entries start here
            foreach (var item in lcCostEntry.LccostEntryDetails)
            {
                //CreditAccountId
                await _accountService.HitAccount(_unitOfWork, lcCostEntry.CostCenterId, lcCostEntry.Code, lcCostEntry.Remark,
                item.CreditAccountId, 0, item.Amount, lcCostEntry.EntryDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.LcCostEntry);
                //DebitAccountId
                await _accountService.HitAccount(_unitOfWork, lcCostEntry.CostCenterId, lcCostEntry.Code, lcCostEntry.Remark,
                item.DebitAccountId, item.Amount, 0, lcCostEntry.EntryDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.LcCostEntry);
            }
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<LccostEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy, x.PurchaseOrderId })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("LC Cost Entry not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This LC Cost Entry does not belong to the current financial year.");

            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == dbInfo.PurchaseOrderId);
            if (dbPurchaseOrder!.Status != (int)PurchaseOrderStatus.Approved) throw new BadRequestException($"Approval Denied: The purchase order {dbPurchaseOrder.Ponumber} linked to this LC Cost Entry has not been approved.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<LccostEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)LCCostEntryStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)LCCostEntryStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"LC Cost Entry already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbLcCostEntry = await _unitOfWork.Repository<LccostEntry>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.LccostEntryDetails));

            dbLcCostEntry.Status = (int)LCCostEntryStatus.Approved;
            dbLcCostEntry.ApprovedBy = approvedBy;

            await InsertTransaction(dbLcCostEntry);
            //update LC Cost Entry
            await _unitOfWork.Repository<LccostEntry>().UpdateAsync(dbLcCostEntry);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbLcCostEntry.Id
                   && x.Status == (int)LCCostEntryStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }

        private async Task DeleteTransaction(string code)
        {
            var dbLcCostEntry = await _unitOfWork.Repository<LccostEntry>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (dbLcCostEntry is null) throw new NotFoundResultException("LC Cost Entry Not Found With this Code");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == dbLcCostEntry.Code);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }
        }

        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<LccostEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("LccostEntry Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This LC Cost Entry does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)LCCostEntryStatus.Checked && dbInfo.Status != (int)LCCostEntryStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)LCCostEntryStatus.Checked
                ? (int)LCCostEntryStatus.Pending
                : (int)LCCostEntryStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<LccostEntry>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbLcCostEntry = await _unitOfWork.Repository<LccostEntry>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.LccostEntryDetails));
            dbLcCostEntry.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)LCCostEntryStatus.Checked:
                    dbLcCostEntry.CheckedBy = "";
                    break;
                case (int)LCCostEntryStatus.Approved:
                    dbLcCostEntry.ApprovedBy = "";
                    await DeleteTransaction(dbLcCostEntry.Code);
                    break;
            }

            await _unitOfWork.Repository<LccostEntry>().UpdateAsync(dbLcCostEntry);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbLcCostEntry.Status;
        }
    }
}
