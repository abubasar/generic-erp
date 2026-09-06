using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Purchase.PoPriceAdjustmentAfterGrn;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PoPriceAdjustmentAfterGrn;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services.Services.Purchase.PoPriceAdjustmentAfterGrns
{
    public class PoPriceAdjustmentAfterGrnService : BaseService<PoPriceAdjustmentAfterGrn, PoPriceAdjustmentAfterGrnCreationDto, PoPriceAdjustmentAfterGrnUpdateDto, PoPriceAdjustmentAfterGrnRequestModel, PoPriceAdjustmentAfterGrnViewModel>, IPoPriceAdjustmentAfterGrnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;

        public PoPriceAdjustmentAfterGrnService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
            IHubContext<BroadcastHub, IHubClient> hubContext, IAccountService accountService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
        }

        public async Task<PoPriceAdjustmentAfterGrnAggregatorModel> PreparePoPriceAdjustmentAfterGrnAggregatorModel(PoPriceAdjustmentAfterGrnRequestModel poPriceAdjustmentAfterGrnRequest)
        {
            var poPriceAdjustmentAfterGrnQueryable = _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().TableNoTracking().Where(poPriceAdjustmentAfterGrnRequest.GetExpression());
            return new PoPriceAdjustmentAfterGrnAggregatorModel
            {
                AggregatorTotalAmount = await poPriceAdjustmentAfterGrnQueryable.SumAsync(x => x.TotalAmount)
            };
        }

        public async Task<PoPriceAdjustmentAfterGrnViewModel> GetByIdAsync(Guid id)
        {
            var poPriceAdjustmentAfterGrn = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Supplier).Include(x => x.Store)
                .Include(x => x.PoPriceAdjustmentAfterGrnDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (poPriceAdjustmentAfterGrn == null) throw new NotFoundResultException("Po Price Adjustment After Grn Not Found With this Id");
            return _mapper.Map<PoPriceAdjustmentAfterGrnViewModel>(poPriceAdjustmentAfterGrn);
        }

        public new async Task<Guid> AddAsync(PoPriceAdjustmentAfterGrnCreationDto poPriceAdjustmentAfterGrnCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            poPriceAdjustmentAfterGrnCreationDto.AdjustmentDate = poPriceAdjustmentAfterGrnCreationDto.AdjustmentDate.ToLocal();
            var poPriceAdjustmentAfterGrn = _mapper.Map<PoPriceAdjustmentAfterGrn>(poPriceAdjustmentAfterGrnCreationDto);
            var count = _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            poPriceAdjustmentAfterGrn.Id = Guid.NewGuid();
            poPriceAdjustmentAfterGrn.Code = "PPA" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            poPriceAdjustmentAfterGrn.FinancialYearId = financialYear.Id;
            poPriceAdjustmentAfterGrn.Status = (int)PoPriceAdjustmentAfterGrnStatus.Pending;

            foreach (var item in poPriceAdjustmentAfterGrn.PoPriceAdjustmentAfterGrnDetails)
            {
                if (item.AdjustmentQuantity == 0) continue;
                item.Id = Guid.NewGuid();
                item.PoPriceAdjustmentAfterGrnId = poPriceAdjustmentAfterGrn.Id;
                await _unitOfWork.Repository<PoPriceAdjustmentAfterGrnDetail>().AddAsync(item);
            }
            poPriceAdjustmentAfterGrn.PoPriceAdjustmentAfterGrnDetails = null;
            await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().AddAsync(poPriceAdjustmentAfterGrn);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, poPriceAdjustmentAfterGrn.Id
                , "/purchase/po-price-adjustment-after-grn/" + poPriceAdjustmentAfterGrn.Id, Permissions.PoPriceAdjustmentAfterGrns.Check,
                "Po Price Adjustment After Grn " + poPriceAdjustmentAfterGrn.Code + " is ready for Check", (int)PoPriceAdjustmentAfterGrnStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return poPriceAdjustmentAfterGrn.Id;
        }

        public new async Task<Guid> UpdateAsync(PoPriceAdjustmentAfterGrnUpdateDto poPriceAdjustmentAfterGrnUpdateDto)
        {
            var dbPoPriceAdjustmentAfterGrn = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().FindAsync(poPriceAdjustmentAfterGrnUpdateDto.Id);
            //when date updated,convert it to local date
            if (poPriceAdjustmentAfterGrnUpdateDto.AdjustmentDate.Equals(dbPoPriceAdjustmentAfterGrn.AdjustmentDate) == false)
            {
                poPriceAdjustmentAfterGrnUpdateDto.AdjustmentDate = poPriceAdjustmentAfterGrnUpdateDto.AdjustmentDate.ToLocal();
            }
            var poPriceAdjustmentAfterGrn = _mapper.Map(poPriceAdjustmentAfterGrnUpdateDto, dbPoPriceAdjustmentAfterGrn);
            if (dbPoPriceAdjustmentAfterGrn.Status == (int)PoPriceAdjustmentAfterGrnStatus.Pending || dbPoPriceAdjustmentAfterGrn.Status == (int)PoPriceAdjustmentAfterGrnStatus.Checked)
            {
                await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().UpdateAsync(poPriceAdjustmentAfterGrn);
                foreach (var item in poPriceAdjustmentAfterGrn.PoPriceAdjustmentAfterGrnDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.PoPriceAdjustmentAfterGrnId = poPriceAdjustmentAfterGrn.Id;
                        await _unitOfWork.Repository<PoPriceAdjustmentAfterGrnDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<PoPriceAdjustmentAfterGrnDetail>().UpdateAsync(item);
                }
                //Delete for PoPriceAdjustmentAfterGrnDetails
                if (!string.IsNullOrEmpty(poPriceAdjustmentAfterGrnUpdateDto.DeletedPoPriceAdjustmentAfterGrnDetailIds))
                {
                    foreach (var id in poPriceAdjustmentAfterGrnUpdateDto.DeletedPoPriceAdjustmentAfterGrnDetailIds.Split(',').Where(x => x != ""))
                    {
                        var poPriceAdjustmentAfterGrnDetail = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrnDetail>().FindAsync(new Guid(id));
                        poPriceAdjustmentAfterGrnDetail.Deleted = true;
                        await _unitOfWork.Repository<PoPriceAdjustmentAfterGrnDetail>().UpdateAsync(poPriceAdjustmentAfterGrnDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return poPriceAdjustmentAfterGrn.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var poPriceAdjustmentAfterGrn = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().FindAsync(x => x.Id == id, x => x.Include(x => x.PoPriceAdjustmentAfterGrnDetails));
            if (poPriceAdjustmentAfterGrn == null) throw new NotFoundResultException("Po Price Adjustment After Grn Not Found With this Id");
            if (poPriceAdjustmentAfterGrn.Status == (int)PoPriceAdjustmentAfterGrnStatus.Pending)
            {
                foreach (var item in poPriceAdjustmentAfterGrn.PoPriceAdjustmentAfterGrnDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<PoPriceAdjustmentAfterGrnDetail>().UpdateAsync(item);
                }
                poPriceAdjustmentAfterGrn.Deleted = true;
                await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().UpdateAsync(poPriceAdjustmentAfterGrn);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return poPriceAdjustmentAfterGrn.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var poPriceAdjustmentAfterGrns = _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await poPriceAdjustmentAfterGrns.Where(x => x.Status == (int)PoPriceAdjustmentAfterGrnStatus.Pending).CountAsync(),
                CheckedCount = await poPriceAdjustmentAfterGrns.Where(x => x.Status == (int)PoPriceAdjustmentAfterGrnStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbPoPriceAdjustmentAfterGrn = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().FindAsync(id);
            if (dbPoPriceAdjustmentAfterGrn.Status == (int)PoPriceAdjustmentAfterGrnStatus.Pending)
            {
                dbPoPriceAdjustmentAfterGrn.Status = (int)PoPriceAdjustmentAfterGrnStatus.Checked;
                dbPoPriceAdjustmentAfterGrn.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().UpdateAsync(dbPoPriceAdjustmentAfterGrn);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPoPriceAdjustmentAfterGrn.Id
                       && x.Status == (int)PoPriceAdjustmentAfterGrnStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbPoPriceAdjustmentAfterGrn.Id
                    , "/purchase/po-price-adjustment-after-grn/" + dbPoPriceAdjustmentAfterGrn.Id, Permissions.PoPriceAdjustmentAfterGrns.Approve,
                    "Po Price Adjustment After Grn " + dbPoPriceAdjustmentAfterGrn.Code + " is ready for Approval", (int)PoPriceAdjustmentAfterGrnStatus.Checked);
                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Po Price Adjustment After Grn Status has already been checked by " + dbPoPriceAdjustmentAfterGrn?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("PO Price Adjustment After GRN not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This PO Price Adjustment After GRN does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)PoPriceAdjustmentAfterGrnStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)PoPriceAdjustmentAfterGrnStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Po Price Adjustment After Grn already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbPoPriceAdjustmentAfterGrn = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.PoPriceAdjustmentAfterGrnDetails).ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));

            dbPoPriceAdjustmentAfterGrn.Status = (int)PoPriceAdjustmentAfterGrnStatus.Approved;
            dbPoPriceAdjustmentAfterGrn.ApprovedBy = approvedBy;

            #region  Insert Transaction
            var variance = 0M;
            foreach (var item in dbPoPriceAdjustmentAfterGrn.PoPriceAdjustmentAfterGrnDetails)
            {
                variance += (item.AdjustmentQuantity * item.GrnRate - item.AdjustmentQuantity * item.AdjustmentRate);
            }
            //update GRN
            await UpdateTotalGrnAdjustmentAmount(dbPoPriceAdjustmentAfterGrn, variance);
            await InsertTransaction(dbPoPriceAdjustmentAfterGrn, variance);
            await InsertSupplierTransactionAgainstPO(dbPoPriceAdjustmentAfterGrn, variance);
            #endregion
            //update PoPriceAdjustmentAfterGrn
            await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().UpdateAsync(dbPoPriceAdjustmentAfterGrn);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPoPriceAdjustmentAfterGrn.Id
                   && x.Status == (int)PoPriceAdjustmentAfterGrnStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.Grnno })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("PO Price Adjustment After GRN not found.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This PO Price Adjustment After GRN does not belong to the current financial year.");
            var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().FindAsync(x => x.Grnno == dbInfo.Grnno);
            if (goodsReceiveNote!.Status == (int)GRNStatus.Invoice_Generated) throw new BadRequestException("Not Possible to Unpost!!!.Purchase Invoice Already Generated with this Adjustment.");
            if (dbInfo.Status != fromStatus) throw new BadRequestException("PO Price Adjustment After GRN status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)PoPriceAdjustmentAfterGrnStatus.Checked && dbInfo.Status != (int)PoPriceAdjustmentAfterGrnStatus.Approved)
                throw new BadRequestException("Only Checked or Approved PO Price Adjustment After GRN can be unposted.");

            var newStatus = dbInfo.Status == (int)PoPriceAdjustmentAfterGrnStatus.Approved
                ? (int)PoPriceAdjustmentAfterGrnStatus.Checked
                : (int)PoPriceAdjustmentAfterGrnStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus));
                    //.SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("PO Price Adjustment After GRN status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbPoPriceAdjustmentAfterGrn = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().FindAsync(id);

            dbPoPriceAdjustmentAfterGrn.Status = newStatus;
            //dbPoPriceAdjustmentAfterGrn.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)PoPriceAdjustmentAfterGrnStatus.Checked:
                    dbPoPriceAdjustmentAfterGrn.CheckedBy = "";
                    break;
                case (int)PoPriceAdjustmentAfterGrnStatus.Approved:
                    dbPoPriceAdjustmentAfterGrn.ApprovedBy = "";
                    #region  Delete Transaction
                    await ReverseTotalGrnAdjustmentAmount(dbPoPriceAdjustmentAfterGrn);
                    await DeleteTransaction(dbPoPriceAdjustmentAfterGrn.Code);
                    await RemoveSupplierTransactionAgainstPO(dbPoPriceAdjustmentAfterGrn);
                    #endregion
                    break;
            }

            await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().UpdateAsync(dbPoPriceAdjustmentAfterGrn);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbPoPriceAdjustmentAfterGrn.Status;
        }

        private async Task InsertSupplierTransactionAgainstPO(PoPriceAdjustmentAfterGrn poPriceAdjustmentAfterGrn, decimal variance)
        {
            var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().SingleOrDefaultAsync(x => x.Ponumber == poPriceAdjustmentAfterGrn.Ponumber);
            if (purchaseOrder is null) throw new BadRequestException("PO not found associated with this GRN NO:" + poPriceAdjustmentAfterGrn.Grnno);
            await _unitOfWork.Repository<SupplierTransactionAgainstPo>().AddAsync(new SupplierTransactionAgainstPo
            {
                Id = Guid.NewGuid(),
                TransactionId = poPriceAdjustmentAfterGrn.Id,
                TransactionDate = poPriceAdjustmentAfterGrn.AdjustmentDate,
                SupplierInvoiceDate = null,
                PaymentTermInDays = 0,
                PurchaseOrderId = purchaseOrder.Id,
                SupplierId = poPriceAdjustmentAfterGrn.SupplierId,
                SupplierTransactionType = (int)SupplierTransactionType.PO_Price_Adjustment_After_GRN,
                Amount = -variance,
                Remark = variance > 0m ? "Grn rate greater than updated rate" : "Grn rate less than updated rate"
            });
        }

        private async Task RemoveSupplierTransactionAgainstPO(PoPriceAdjustmentAfterGrn poPriceAdjustmentAfterGrn)
        {
            var transactions = _unitOfWork.Repository<SupplierTransactionAgainstPo>().TableNoTracking().Where(x => x.TransactionId == poPriceAdjustmentAfterGrn.Id);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<SupplierTransactionAgainstPo>().DeleteAsync(transaction.Id);
                }
            }
        }

        private async Task InsertTransaction(PoPriceAdjustmentAfterGrn poPriceAdjustmentAfterGrn, decimal variance)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(poPriceAdjustmentAfterGrn.Ponumber)
              .Append(", ")
              .Append(poPriceAdjustmentAfterGrn.Grnno);
            foreach (var item in poPriceAdjustmentAfterGrn.PoPriceAdjustmentAfterGrnDetails)
            {
                sb.Append(", ")
                  .Append(item.Product.Name)
                  .Append(" ")
                  .Append(item.AdjustmentQuantity)
                  .Append(item.Product.MeasurementUnit.Name)
                  .Append(" ")
                  .Append(item.Amount)
                  .Append(" BDT");
            }
            string description = sb.ToString();
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var supplierAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == poPriceAdjustmentAfterGrn.SupplierId).Select(x => x.Name).FirstOrDefaultAsync();
            var purchasePriceVarianceAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchasePriceVariance.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            if (variance > 0M)
            {
                contraAccountNamesForCredit.Append(supplierAccountName);
                contraAccountIdsForCredit.Append(poPriceAdjustmentAfterGrn.SupplierId.ToString());
                contraAccountNamesForDebit.Append(purchasePriceVarianceAccountName);
                contraAccountIdsForDebit.Append(AccountHeadConstants.PurchasePriceVariance);
            }
            else
            {
                contraAccountNamesForCredit.Append(purchasePriceVarianceAccountName);
                contraAccountIdsForCredit.Append(AccountHeadConstants.PurchasePriceVariance);
                contraAccountNamesForDebit.Append(supplierAccountName);
                contraAccountIdsForDebit.Append(poPriceAdjustmentAfterGrn.SupplierId.ToString());
            }
            //when grn rate is greater then updated rate(income)
            if (variance > 0M)
            {
                //party a/c debit
                await _accountService.HitAccount(_unitOfWork, null, poPriceAdjustmentAfterGrn.Code, description,
                    poPriceAdjustmentAfterGrn.SupplierId, variance, 0, poPriceAdjustmentAfterGrn.AdjustmentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

                //purchase price variance credit
                await _accountService.HitAccount(_unitOfWork, null, poPriceAdjustmentAfterGrn.Code, description,
                      Guid.Parse(AccountHeadConstants.PurchasePriceVariance), 0, variance, poPriceAdjustmentAfterGrn.AdjustmentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            }
            //when grn rate is less then updated rate(loss)
            else
            {
                //party a/c credit
                await _accountService.HitAccount(_unitOfWork, null, poPriceAdjustmentAfterGrn.Code, description,
                    poPriceAdjustmentAfterGrn.SupplierId, 0, Math.Abs(variance), poPriceAdjustmentAfterGrn.AdjustmentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

                //purchase price variance debit
                await _accountService.HitAccount(_unitOfWork, null, poPriceAdjustmentAfterGrn.Code, description,
                      Guid.Parse(AccountHeadConstants.PurchasePriceVariance), Math.Abs(variance), 0, poPriceAdjustmentAfterGrn.AdjustmentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

            }
        }

        private async Task DeleteTransaction(string code)
        {
            var poPriceAdjustmentAfterGrn = await _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (poPriceAdjustmentAfterGrn is null) throw new NotFoundResultException("Po Price Adjustment After Grn Not Found With this Code");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == poPriceAdjustmentAfterGrn.Code);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }

        }

        private async Task UpdateTotalGrnAdjustmentAmount(PoPriceAdjustmentAfterGrn poPriceAdjustmentAfterGrn, decimal variance)
        {
            var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().FindAsync(x => x.Grnno == poPriceAdjustmentAfterGrn.Grnno);
            if (goodsReceiveNote is null) throw new NotFoundResultException("GRN not found");
            goodsReceiveNote.TotalGrnAdjustmentAmount = variance;
            await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(goodsReceiveNote);

        }

        private async Task ReverseTotalGrnAdjustmentAmount(PoPriceAdjustmentAfterGrn poPriceAdjustmentAfterGrn)
        {
            var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().FindAsync(x => x.Grnno == poPriceAdjustmentAfterGrn.Grnno);
            if (goodsReceiveNote is null) throw new NotFoundResultException("GRN not found");
            goodsReceiveNote.TotalGrnAdjustmentAmount = 0;
            await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(goodsReceiveNote);
        }

    }
}
