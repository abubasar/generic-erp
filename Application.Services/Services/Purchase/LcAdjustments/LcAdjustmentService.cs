using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Purchase.LcAdjustment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Purchase.LcAdjustments
{
    public class LcAdjustmentService : BaseService<LcAdjustment, LcAdjustmentCreationDto, LcAdjustmentUpdateDto, LcAdjustmentRequestModel, LcAdjustmentViewModel>, ILcAdjustmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        public LcAdjustmentService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService, IAccountService accountService, IHubContext<BroadcastHub, IHubClient> hubContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _accountService = accountService;
            _hubContext = hubContext;
        }

        public async Task<LcAdjustmentViewModel> GetByIdAsync(Guid id)
        {
            var lcAdjustment = await _unitOfWork.Repository<LcAdjustment>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.PurchaseInvoice).Include(x => x.LcAdjustmentDetails).ThenInclude(x => x.Account));
            if (lcAdjustment == null) throw new NotFoundResultException("LC Adjustment Not Found With this Id");
            return _mapper.Map<LcAdjustmentViewModel>(lcAdjustment);
        }

        public new async Task<Guid> AddAsync(LcAdjustmentCreationDto lcAdjustmentCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var dbPurchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(lcAdjustmentCreationDto.PurchaseInvoiceId);
            if(dbPurchaseInvoice.IsLcAdjusted) throw new BadRequestException("This Purchase Invoice has already been LC Adjusted.");
            if (dbPurchaseInvoice == null) throw new NotFoundResultException("Purchase Invoice Not Found With this Id");
            dbPurchaseInvoice.IsLcAdjusted = true;
            lcAdjustmentCreationDto.AdjustmentDate = lcAdjustmentCreationDto.AdjustmentDate.ToLocal();
            var lcAdjustment = _mapper.Map<LcAdjustment>(lcAdjustmentCreationDto);
            var count = _unitOfWork.Repository<LcAdjustment>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            lcAdjustment.Id = Guid.NewGuid();
            lcAdjustment.Code = "LCA" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            lcAdjustment.FinancialYearId = financialYear.Id;
            lcAdjustment.Status = (int)LcAdjustmentStatus.Pending;
            foreach (var item in lcAdjustment.LcAdjustmentDetails)
            {
                item.Id = Guid.NewGuid();
                item.LcAdjustmentId = lcAdjustment.Id;
                await _unitOfWork.Repository<LcAdjustmentDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<LcAdjustment>().AddAsync(lcAdjustment);
            await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(dbPurchaseInvoice);

            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, lcAdjustment.Id
                , "/purchase/lc-adjustment/" + lcAdjustment.Id, Permissions.LcAdjustments.Check,
                "LC Adjustment " + lcAdjustment.Code + " is ready for Check", (int)LcAdjustmentStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return lcAdjustment.Id;
        }

        public new async Task<Guid> UpdateAsync(LcAdjustmentUpdateDto lcAdjustmentUpdateDto)
        {
            var dbLcAdjustment = await _unitOfWork.Repository<LcAdjustment>().FindAsync(lcAdjustmentUpdateDto.Id);
            if (dbLcAdjustment.PurchaseInvoiceId != lcAdjustmentUpdateDto.PurchaseInvoiceId)
            {
                var dbPurchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(lcAdjustmentUpdateDto.PurchaseInvoiceId);
                if (dbPurchaseInvoice == null) throw new NotFoundResultException("Purchase Invoice Not Found With this Id");
                dbPurchaseInvoice.IsLcAdjusted = true;
                await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(dbPurchaseInvoice);
            }
            //when date updated,convert it to local date
            if (lcAdjustmentUpdateDto.AdjustmentDate.Equals(dbLcAdjustment.AdjustmentDate) == false)
            {
                lcAdjustmentUpdateDto.AdjustmentDate = lcAdjustmentUpdateDto.AdjustmentDate.ToLocal();
            }
            var lcAdjustment = _mapper.Map(lcAdjustmentUpdateDto, dbLcAdjustment);
            if (dbLcAdjustment.Status == (int)LcAdjustmentStatus.Pending || dbLcAdjustment.Status == (int)LcAdjustmentStatus.Checked)
            {
                await _unitOfWork.Repository<LcAdjustment>().UpdateAsync(lcAdjustment);
                foreach (var item in lcAdjustment.LcAdjustmentDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.LcAdjustmentId = lcAdjustment.Id;
                        await _unitOfWork.Repository<LcAdjustmentDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<LcAdjustmentDetail>().UpdateAsync(item);
                }
                //Delete for LcAdjustmentDetails
                if (!string.IsNullOrEmpty(lcAdjustmentUpdateDto.DeletedLcAdjustmentDetailIds))
                {
                    foreach (var id in lcAdjustmentUpdateDto.DeletedLcAdjustmentDetailIds.Split(',').Where(x => x != ""))
                    {
                        var lccostEntryDetail = await _unitOfWork.Repository<LcAdjustmentDetail>().FindAsync(new Guid(id));
                        lccostEntryDetail.Deleted = true;
                        await _unitOfWork.Repository<LcAdjustmentDetail>().UpdateAsync(lccostEntryDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return lcAdjustment.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var lcAdjustment = await _unitOfWork.Repository<LcAdjustment>().FindAsync(x => x.Id == id, x => x.Include(x => x.LcAdjustmentDetails));
            if (lcAdjustment == null) throw new NotFoundResultException("LC Adjustment Not Found With this Id");
            if (lcAdjustment.Status == (int)LcAdjustmentStatus.Pending)
            {
                var dbPurchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(lcAdjustment.PurchaseInvoiceId);
                if (dbPurchaseInvoice == null) throw new NotFoundResultException("Purchase Invoice Not Found With this Id");
                dbPurchaseInvoice.IsLcAdjusted = false;
                await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(dbPurchaseInvoice);
                foreach (var item in lcAdjustment.LcAdjustmentDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<LcAdjustmentDetail>().UpdateAsync(item);
                }
                lcAdjustment.Deleted = true;
                await _unitOfWork.Repository<LcAdjustment>().UpdateAsync(lcAdjustment);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return lcAdjustment.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var lcAdjustments = _unitOfWork.Repository<LcAdjustment>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await lcAdjustments.Where(x => x.Status == (int)LcAdjustmentStatus.Pending).CountAsync(),
                CheckedCount = await lcAdjustments.Where(x => x.Status == (int)LcAdjustmentStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbLcAdjustment = await _unitOfWork.Repository<LcAdjustment>().FindAsync(id);
            if (dbLcAdjustment.Status == (int)LcAdjustmentStatus.Pending)
            {
                dbLcAdjustment.Status = (int)LcAdjustmentStatus.Checked;
                dbLcAdjustment.CheckedBy = _workContext.GetUserName();
                //update LcAdjustment
                await _unitOfWork.Repository<LcAdjustment>().UpdateAsync(dbLcAdjustment);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbLcAdjustment.Id
                       && x.Status == (int)LcAdjustmentStatus.Pending);

                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbLcAdjustment.Id
                    , "/purchase/lc-adjustment/" + dbLcAdjustment.Id, Permissions.LcAdjustments.Approve,
                    "LC Adjustment " + dbLcAdjustment.Code + " is ready for Approval", (int)LcAdjustmentStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("LC Adjustment Status has already been checked by " + dbLcAdjustment?.CheckedBy);
        }

        private async Task InsertTransaction(LcAdjustment lcAdjustment)
        {
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var counterCredit = 0M;
            var counterDebit = 0M;
            foreach (var item in lcAdjustment.LcAdjustmentDetails)
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
            if (lcAdjustment.LcAdjustmentDetails.Where(x => x.PostType == 1).Sum(x => x.Amount) !=
                lcAdjustment.LcAdjustmentDetails.Where(x => x.PostType == 2).Sum(x => x.Amount)) throw new BadRequestException("Debit Credit Not Equal!");
            foreach (var item in lcAdjustment.LcAdjustmentDetails)
            {
                await _accountService.HitAccount(_unitOfWork, lcAdjustment.CostCenterId, lcAdjustment.Code, lcAdjustment.Remark,
                item.AccountId, item.PostType == 1 ? item.Amount : 0, item.PostType == 1 ? 0 : item.Amount, lcAdjustment.AdjustmentDate, item.PostType == 1 ? contraAccountIdsForDebit.ToString() : contraAccountIdsForCredit.ToString(), item.PostType == 1 ? contraAccountNamesForDebit.ToString() : contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.LcAdjustment);
            }
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<LcAdjustment>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("LC Adjustment not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This LC Adjustment does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<LcAdjustment>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)LcAdjustmentStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)LcAdjustmentStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"LC Adjustment already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbLcAdjustment = await _unitOfWork.Repository<LcAdjustment>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.LcAdjustmentDetails));

            dbLcAdjustment.Status = (int)LcAdjustmentStatus.Approved;
            dbLcAdjustment.ApprovedBy = approvedBy;

            await InsertTransaction(dbLcAdjustment);
            //update LC Adjustment
            await _unitOfWork.Repository<LcAdjustment>().UpdateAsync(dbLcAdjustment);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbLcAdjustment.Id
                   && x.Status == (int)LcAdjustmentStatus.Checked);

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
            var lcAdjustment = await _unitOfWork.Repository<LcAdjustment>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (lcAdjustment is null) throw new NotFoundResultException("LC Adjustment Not Found With this Code");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == lcAdjustment.Code);
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
            var dbInfo = await _unitOfWork.Repository<LcAdjustment>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("LcAdjustment Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This LC Adjustment does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)LcAdjustmentStatus.Checked && dbInfo.Status != (int)LcAdjustmentStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)LcAdjustmentStatus.Checked
                ? (int)LcAdjustmentStatus.Pending
                : (int)LcAdjustmentStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<LcAdjustment>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbLcAdjustment = await _unitOfWork.Repository<LcAdjustment>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.LcAdjustmentDetails));
            dbLcAdjustment.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)LcAdjustmentStatus.Checked:
                    dbLcAdjustment.CheckedBy = "";
                    break;
                case (int)LcAdjustmentStatus.Approved:
                    dbLcAdjustment.ApprovedBy = "";
                    await DeleteTransaction(dbLcAdjustment.Code);
                    break;
            }

            await _unitOfWork.Repository<LcAdjustment>().UpdateAsync(dbLcAdjustment);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbLcAdjustment.Status;
        }
    }
}
