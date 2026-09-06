using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.FundTransfer;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.FundTransfer;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services.Services.Accounts.FundTransfers
{
    public class FundTransferService : BaseService<FundTransfer, FundTransferCreationDto, FundTransferUpdateDto, FundTransferRequestModel, FundTransferViewModel>, IFundTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;

        public FundTransferService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService, IAccountService accountService, IHubContext<BroadcastHub, IHubClient> hubContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _accountService = accountService;
            _hubContext = hubContext;
        }

        public virtual async Task<FundTransferAggregatorModel> PrepareFundTransferAggregatorModel(FundTransferRequestModel request)
        {
            var fundTransfersQueryable = _unitOfWork.Repository<FundTransfer>().TableNoTracking().Where(request.GetExpression());
            return new FundTransferAggregatorModel
            {
                AggregatorAmount = await fundTransfersQueryable.SumAsync(x => x.Amount),
                AggregatorCharges = await fundTransfersQueryable.SumAsync(x => x.Charges),
            };
        }

        public async Task<FundTransferViewModel> GetByIdAsync(Guid id)
        {
            var fundTransfer = await _unitOfWork.Repository<FundTransfer>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.TransferFromAccount).Include(x => x.TransferToAccount));
            if (fundTransfer == null) throw new NotFoundResultException("Fund Transfer Not Found With this Id");
            return _mapper.Map<FundTransferViewModel>(fundTransfer);
        }

        public new async Task<AddUpdateResponseModel> AddAsync(FundTransferCreationDto fundTransferCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            fundTransferCreationDto.FundTransferDate = fundTransferCreationDto.FundTransferDate.ToLocal();
            var fundTransfer = _mapper.Map<FundTransfer>(fundTransferCreationDto);
            var count = _unitOfWork.Repository<FundTransfer>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            fundTransfer.Id = Guid.NewGuid();
            fundTransfer.FundTransferNo = "FT" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            fundTransfer.FinancialYearId = financialYear.Id;
            fundTransfer.Status = (int)FundTransferStatus.Pending;

            await _unitOfWork.Repository<FundTransfer>().AddAsync(fundTransfer);

            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, fundTransfer.Id
                , "/account/fund-transfer/" + fundTransfer.Id, Permissions.FundTransfers.Check,
                "Fund Transfer " + fundTransfer.FundTransferNo + " is ready for Check", (int)FundTransferStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return new AddUpdateResponseModel { Id = fundTransfer.Id, Code = fundTransfer.FundTransferNo, Status = fundTransfer.Status };
        }

        public new async Task<AddUpdateResponseModel> UpdateAsync(FundTransferUpdateDto fundTransferUpdateDto)
        {
            var dbFundTransfer = await _unitOfWork.Repository<FundTransfer>().FindAsync(fundTransferUpdateDto.Id);
            //when date updated,convert it to local date
            if (fundTransferUpdateDto.FundTransferDate.Equals(dbFundTransfer.FundTransferDate) == false)
            {
                fundTransferUpdateDto.FundTransferDate = fundTransferUpdateDto.FundTransferDate.ToLocal();
            }
            var fundTransfer = _mapper.Map(fundTransferUpdateDto, dbFundTransfer);
            if (dbFundTransfer.Status == (int)FundTransferStatus.Pending || dbFundTransfer.Status == (int)FundTransferStatus.Checked)
            {
                await _unitOfWork.Repository<FundTransfer>().UpdateAsync(fundTransfer);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return new AddUpdateResponseModel { Id = fundTransfer.Id, Code = fundTransfer.FundTransferNo, Status = fundTransfer.Status };
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var fundTransfer = await _unitOfWork.Repository<FundTransfer>().FindAsync(x => x.Id == id);
            if (fundTransfer == null) throw new NotFoundResultException("Fund Transfer Not Found With this Id");
            if (fundTransfer.Status == (int)FundTransferStatus.Pending)
            {
                fundTransfer.Deleted = true;
                await _unitOfWork.Repository<FundTransfer>().UpdateAsync(fundTransfer);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return fundTransfer.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var fundTransfers = _unitOfWork.Repository<FundTransfer>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await fundTransfers.Where(x => x.Status == (int)FundTransferStatus.Pending).CountAsync(),
                CheckedCount = await fundTransfers.Where(x => x.Status == (int)FundTransferStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbFundTransfer = await _unitOfWork.Repository<FundTransfer>().FindAsync(id);
            if (dbFundTransfer.Status == (int)FundTransferStatus.Pending)
            {
                dbFundTransfer.Status = (int)FundTransferStatus.Checked;
                dbFundTransfer.CheckedBy = _workContext.GetUserName();
                //update FundTransfer
                await _unitOfWork.Repository<FundTransfer>().UpdateAsync(dbFundTransfer);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbFundTransfer.Id
                       && x.Status == (int)FundTransferStatus.Pending);

                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbFundTransfer.Id
                    , "/account/fund-transfer/" + dbFundTransfer.Id, Permissions.FundTransfers.Approve,
                    "Fund Transfer " + dbFundTransfer.FundTransferNo + " is ready for Approval", (int)FundTransferStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Fund Transfer Status has already been checked by " + dbFundTransfer?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<FundTransfer>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Fund Transfer not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Fund Transfer does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<FundTransfer>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)FundTransferStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)FundTransferStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Fund Transfer already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbFundTransfer = await _unitOfWork.Repository<FundTransfer>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.CostCenter).Include(x => x.TransferFromAccount).Include(x => x.TransferToAccount));

            dbFundTransfer.Status = (int)FundTransferStatus.Approved;
            dbFundTransfer.ApprovedBy = approvedBy;

            await InsertTransaction(dbFundTransfer);
            //update Fund Transfer
            await _unitOfWork.Repository<FundTransfer>().UpdateAsync(dbFundTransfer);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbFundTransfer.Id
                   && x.Status == (int)FundTransferStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<FundTransfer>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("FundTransfer Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Fund Transfer does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)FundTransferStatus.Checked && dbInfo.Status != (int)FundTransferStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)FundTransferStatus.Checked
                ? (int)FundTransferStatus.Pending
                : (int)FundTransferStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<FundTransfer>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbFundTransfer = await _unitOfWork.Repository<FundTransfer>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.CostCenter).Include(x => x.TransferFromAccount).Include(x => x.TransferToAccount));
            dbFundTransfer.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)FundTransferStatus.Checked:
                    dbFundTransfer.CheckedBy = "";
                    break;
                case (int)FundTransferStatus.Approved:
                    dbFundTransfer.ApprovedBy = "";
                    await DeleteTransaction(dbFundTransfer.FundTransferNo);
                    break;
            }

            await _unitOfWork.Repository<FundTransfer>().UpdateAsync(dbFundTransfer);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbFundTransfer.Status;
        }

        private async Task InsertTransaction(FundTransfer fundTransfer)
        {
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var transferFromAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == fundTransfer.TransferFromAccountId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(transferFromAccountName);
            contraAccountIdsForDebit.Append(fundTransfer.TransferFromAccountId.ToString());
            var transferToAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == fundTransfer.TransferToAccountId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(transferToAccountName);
            contraAccountIdsForCredit.Append(fundTransfer.TransferToAccountId.ToString());
            if (fundTransfer.Charges > 0m)
            {
                contraAccountNamesForDebit.Append(", " + transferFromAccountName);
                contraAccountIdsForDebit.Append(", " + fundTransfer.TransferFromAccountId.ToString());
                var bankChargeAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.BankCharge.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForCredit.Append(", " + bankChargeAccountName);
                contraAccountIdsForCredit.Append(", " + AccountHeadConstants.BankCharge);
            }
            //Transaction entries start here
            //from account credit
            await _accountService.HitAccount(_unitOfWork, fundTransfer.CostCenterId, fundTransfer.FundTransferNo, fundTransfer.Remark,
            fundTransfer.TransferFromAccountId, 0, fundTransfer.Amount, fundTransfer.FundTransferDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.FundTransfer);
            //from account debit
            await _accountService.HitAccount(_unitOfWork, fundTransfer.CostCenterId, fundTransfer.FundTransferNo, fundTransfer.Remark,
            fundTransfer.TransferToAccountId, fundTransfer.Amount, 0, fundTransfer.FundTransferDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.FundTransfer);

            if (fundTransfer.Charges > 0m)
            {
                //from account credit
                await _accountService.HitAccount(_unitOfWork, fundTransfer.CostCenterId, fundTransfer.FundTransferNo, "Bank Charge for Fund Transfer  " + fundTransfer.Remark,
                fundTransfer.TransferFromAccountId, 0, fundTransfer.Charges, fundTransfer.FundTransferDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.FundTransfer);
                //Bank Charge Account debit
                await _accountService.HitAccount(_unitOfWork, fundTransfer.CostCenterId, fundTransfer.FundTransferNo, "Bank Charge for Fund Transfer " + fundTransfer.Remark,
                AccountHeadConstants.BankCharge.ToGuid(), fundTransfer.Charges, 0, fundTransfer.FundTransferDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.FundTransfer);

            }

        }

        private async Task DeleteTransaction(string code)
        {
            var fundTransfer = await _unitOfWork.Repository<FundTransfer>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.FundTransferNo == code);
            if (fundTransfer is null) throw new NotFoundResultException("Fund Transfer Not Found With this voucher No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == fundTransfer.FundTransferNo);
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
