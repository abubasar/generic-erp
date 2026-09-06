using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Purchase.PurchaseReturn;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseReturn;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services.Services.Purchase.PurchaseReturns
{
    public class PurchaseReturnService : BaseService<PurchaseReturn, PurchaseReturnCreationDto, PurchaseReturnUpdateDto, PurchaseReturnRequestModel, PurchaseReturnViewModel>, IPurchaseReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ICogsCalculationService _cogsCalculationService;

        public PurchaseReturnService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
            IHubContext<BroadcastHub, IHubClient> hubContext, IAccountService accountService, ICogsCalculationService cogsCalculationService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
            _cogsCalculationService = cogsCalculationService;
        }

        public async Task<PurchaseReturnAggregatorModel> PreparePurchaseReturnAggregatorModel(PurchaseReturnRequestModel purchaseReturnRequest)
        {
            var purchaseReturnQueryable = _unitOfWork.Repository<PurchaseReturn>().TableNoTracking().Where(purchaseReturnRequest.GetExpression());
            return new PurchaseReturnAggregatorModel
            {
                AggregatorTotalAmount = await purchaseReturnQueryable.SumAsync(x => x.TotalAmount)
            };
        }

        public async Task<PurchaseReturnViewModel> GetByIdAsync(Guid id)
        {
            var purchaseReturn = await _unitOfWork.Repository<PurchaseReturn>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Supplier).Include(x => x.Store)
                .Include(x => x.PurchaseReturnDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (purchaseReturn == null) throw new NotFoundResultException("Purchase Return Not Found With this Id");
            return _mapper.Map<PurchaseReturnViewModel>(purchaseReturn);
        }

        public new async Task<Guid> AddAsync(PurchaseReturnCreationDto purchaseReturnCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            purchaseReturnCreationDto.PurchaseReturnDate = purchaseReturnCreationDto.PurchaseReturnDate.ToLocal();
            var purchaseReturn = _mapper.Map<PurchaseReturn>(purchaseReturnCreationDto);
            var count = _unitOfWork.Repository<PurchaseReturn>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            purchaseReturn.Id = Guid.NewGuid();
            purchaseReturn.PurchaseReturnNo = "R" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            purchaseReturn.FinancialYearId = financialYear.Id;
            purchaseReturn.Status = (int)PurchaseReturnStatus.Pending;

            foreach (var item in purchaseReturn.PurchaseReturnDetails)
            {
                if (item.NetQuantity == 0) continue;
                item.Id = Guid.NewGuid();
                item.PurchaseReturnId = purchaseReturn.Id;
                await _unitOfWork.Repository<PurchaseReturnDetail>().AddAsync(item);
            }
            purchaseReturn.PurchaseReturnDetails = null;
            await _unitOfWork.Repository<PurchaseReturn>().AddAsync(purchaseReturn);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, purchaseReturn.Id
                , "/purchase/purchase-return/" + purchaseReturn.Id, Permissions.PurchaseReturns.Check,
                "Purchase Return " + purchaseReturn.PurchaseReturnNo + " is ready for Check", (int)PurchaseReturnStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return purchaseReturn.Id;
        }

        public new async Task<Guid> UpdateAsync(PurchaseReturnUpdateDto purchaseReturnUpdateDto)
        {
            var dbPurchaseReturn = await _unitOfWork.Repository<PurchaseReturn>().FindAsync(purchaseReturnUpdateDto.Id);
            //when date updated,convert it to local date
            if (purchaseReturnUpdateDto.PurchaseReturnDate.Equals(dbPurchaseReturn.PurchaseReturnDate) == false)
            {
                purchaseReturnUpdateDto.PurchaseReturnDate = purchaseReturnUpdateDto.PurchaseReturnDate.ToLocal();
            }
            var purchaseReturn = _mapper.Map(purchaseReturnUpdateDto, dbPurchaseReturn);
            if (dbPurchaseReturn.Status == (int)PurchaseReturnStatus.Pending || dbPurchaseReturn.Status == (int)PurchaseReturnStatus.Checked)
            {
                await _unitOfWork.Repository<PurchaseReturn>().UpdateAsync(purchaseReturn);
                foreach (var item in purchaseReturn.PurchaseReturnDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.PurchaseReturnId = purchaseReturn.Id;
                        await _unitOfWork.Repository<PurchaseReturnDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<PurchaseReturnDetail>().UpdateAsync(item);
                }
                //Delete for PurchaseReturnDetails
                if (!string.IsNullOrEmpty(purchaseReturnUpdateDto.DeletedPurchaseReturnDetailIds))
                {
                    foreach (var id in purchaseReturnUpdateDto.DeletedPurchaseReturnDetailIds.Split(',').Where(x => x != ""))
                    {
                        var purchaseReturnDetail = await _unitOfWork.Repository<PurchaseReturnDetail>().FindAsync(new Guid(id));
                        purchaseReturnDetail.Deleted = true;
                        await _unitOfWork.Repository<PurchaseReturnDetail>().UpdateAsync(purchaseReturnDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return purchaseReturn.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var purchaseReturn = await _unitOfWork.Repository<PurchaseReturn>().FindAsync(x => x.Id == id, x => x.Include(x => x.PurchaseReturnDetails));
            if (purchaseReturn == null) throw new NotFoundResultException("Purchase Return Not Found With this Id");
            if (purchaseReturn.Status == (int)PurchaseReturnStatus.Pending)
            {
                foreach (var item in purchaseReturn.PurchaseReturnDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<PurchaseReturnDetail>().UpdateAsync(item);
                }
                purchaseReturn.Deleted = true;
                await _unitOfWork.Repository<PurchaseReturn>().UpdateAsync(purchaseReturn);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return purchaseReturn.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var purchaseReturns = _unitOfWork.Repository<PurchaseReturn>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await purchaseReturns.Where(x => x.Status == (int)PurchaseReturnStatus.Pending).CountAsync(),
                CheckedCount = await purchaseReturns.Where(x => x.Status == (int)PurchaseReturnStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbPurchaseReturn = await _unitOfWork.Repository<PurchaseReturn>().FindAsync(id);
            if (dbPurchaseReturn.Status == (int)PurchaseReturnStatus.Pending)
            {
                dbPurchaseReturn.Status = (int)PurchaseReturnStatus.Checked;
                dbPurchaseReturn.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<PurchaseReturn>().UpdateAsync(dbPurchaseReturn);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseReturn.Id
                       && x.Status == (int)PurchaseReturnStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbPurchaseReturn.Id
                    , "/purchase/purchase-return/" + dbPurchaseReturn.Id, Permissions.PurchaseReturns.Approve,
                    "Purchase Return " + dbPurchaseReturn.PurchaseReturnNo + " is ready for Approval", (int)PurchaseReturnStatus.Checked);
                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Purchase Return Status has already been checked by " + dbPurchaseReturn?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<PurchaseReturn>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Purchase Return not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Purchase Return does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<PurchaseReturn>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)PurchaseReturnStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)PurchaseReturnStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Purchase Return already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbPurchaseReturn = await _unitOfWork.Repository<PurchaseReturn>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.PurchaseReturnDetails).ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));

            dbPurchaseReturn.Status = (int)PurchaseReturnStatus.Approved;
            dbPurchaseReturn.ApprovedBy = approvedBy;

            #region Stock Out and Insert Transaction
            decimal rmCostTotal = 0.0M;
            if (string.IsNullOrEmpty(dbPurchaseReturn.Grnno))
            {

                foreach (var item in dbPurchaseReturn.PurchaseReturnDetails)
                {
                    decimal rmCost = await StockOutFIFO(dbPurchaseReturn, item);
                    rmCostTotal += rmCost;
                }
            }
            else
            {
                var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking().SingleOrDefaultAsync(x => x.Grnno == dbPurchaseReturn.Grnno);
                if (goodsReceiveNote is null) throw new NotFoundResultException("Goods Receive Note Not Found With grn no");
                foreach (var item in dbPurchaseReturn.PurchaseReturnDetails)
                {
                    decimal rmCost = await StockOutAgainstGRN(dbPurchaseReturn, goodsReceiveNote.Id, item.ProductId, item.NetQuantity);
                    rmCostTotal += rmCost;
                }
            }
            await InsertTransaction(dbPurchaseReturn, rmCostTotal);
            await InsertSupplierTransactionAgainstPO(dbPurchaseReturn);
            #endregion
            foreach (var item in dbPurchaseReturn.PurchaseReturnDetails)
            {
                item.Product = null;
            }
            //update PurchaseReturn
            await _unitOfWork.Repository<PurchaseReturn>().UpdateAsync(dbPurchaseReturn);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseReturn.Id
                   && x.Status == (int)PurchaseReturnStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<PurchaseReturn>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Purchase Return not found.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Purchase Return does not belong to the current financial year.");
            if (dbInfo.Status != fromStatus) throw new BadRequestException("Purchase Return status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)PurchaseReturnStatus.Checked && dbInfo.Status != (int)PurchaseReturnStatus.Approved)
                throw new BadRequestException("Only Checked or Approved Purchase Return can be unposted.");

            var newStatus = dbInfo.Status == (int)PurchaseReturnStatus.Approved
                ? (int)PurchaseReturnStatus.Checked
                : (int)PurchaseReturnStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<PurchaseReturn>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus));
                    //.SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Purchase Return status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbPurchaseReturn = await _unitOfWork.Repository<PurchaseReturn>().FindAsync(id);

            dbPurchaseReturn.Status = newStatus;
            //dbPurchaseReturn.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)PurchaseReturnStatus.Checked:
                    dbPurchaseReturn.CheckedBy = "";
                    break;
                case (int)PurchaseReturnStatus.Approved:
                    dbPurchaseReturn.ApprovedBy = "";
                    #region Reverse Stock and Delete Transaction
                    await ReverseStock(dbPurchaseReturn);
                    await DeleteTransaction(dbPurchaseReturn.PurchaseReturnNo);
                    await RemoveSupplierTransactionAgainstPO(dbPurchaseReturn);
                    #endregion
                    break;
            }

            await _unitOfWork.Repository<PurchaseReturn>().UpdateAsync(dbPurchaseReturn);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbPurchaseReturn.Status;
        }
        private async Task<decimal> StockOutFIFO(PurchaseReturn purchaseReturn, PurchaseReturnDetail item)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
            var (rmcost, stocks) = _cogsCalculationService.CalculateCOGS(new List<int> { (int)TransactonType.Purchase, (int)TransactonType.StockAdjustment_Plus }, product, purchaseReturn.StoreId, item.NetQuantity);
            foreach (var stock in stocks)
            {
                await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
            }

            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.PurchaseReturn,
                InventoryTypeId = product.InventoryTypeId,
                ProductTypeId = product.ProductTypeId,
                ProductId = item.ProductId,
                StoreId = purchaseReturn.StoreId,
                OutQty = item.NetQuantity,
                OutRate = rmcost / item.NetQuantity,
                BatchNo = string.Join(",", stocks.Select(x => x.BatchNo)),
                TransactionId = purchaseReturn.Id,
                StockInDate = stocks.First().StockInDate,
                TransactionDate = purchaseReturn.PurchaseReturnDate,
                Remark = "Purchase Return Without GRN" + purchaseReturn?.PurchaseReturnNo,
                FinancialYearId = financialYear.Id
            });
            return rmcost;
        }

        private async Task<decimal> StockOutAgainstGRN(PurchaseReturn purchaseReturn, Guid goodsReceiveNoteId, Guid productId, int returnQty)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == productId);
            var returnStock = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .SingleOrDefaultAsync(x => x.TransactonType == (int)TransactonType.Purchase && x.TransactionId == purchaseReturn.Id && x.ProductId == productId);
            if (returnStock is not null)
            {
                //update stock
                if (returnQty > returnStock.InQty) throw new BadRequestException($"Return Qry {returnQty} is greater than stock quantity {returnStock.InQty} !!");
                if (returnQty > returnStock.AvailableQty) throw new BadRequestException($"Return quantity {returnQty} exceeds available stock quantity {returnStock.AvailableQty}");
                returnStock.AvailableQty = returnStock.AvailableQty - returnQty;
                await _unitOfWork.Repository<Stock>().UpdateAsync(returnStock);
                //return stock
                await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                {
                    Id = Guid.NewGuid(),
                    TransactonType = (int)TransactonType.PurchaseReturn,
                    InventoryTypeId = product.InventoryTypeId,
                    ProductTypeId = product.ProductTypeId,
                    ProductId = returnStock.ProductId,
                    StoreId = returnStock.StoreId,
                    OutQty = returnQty,
                    OutRate = returnStock.InRate + returnStock.InTransportCost,
                    BatchNo = returnStock.BatchNo,
                    TransactionId = purchaseReturn.Id,
                    StockInDate = returnStock.StockInDate,
                    TransactionDate = purchaseReturn.PurchaseReturnDate,
                    Remark = "Purchase Return Against " + purchaseReturn?.Grnno,
                    FinancialYearId = financialYear.Id
                });
                return returnQty * (returnStock.InRate + returnStock.InTransportCost);
            }
            else
            {
                var stock = await _unitOfWork.Repository<Stock>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.TransactonType == (int)TransactonType.Purchase && x.TransactionId == goodsReceiveNoteId && x.ProductId == productId);
                if (stock is null) throw new NotFoundResultException("Stock  Not Found");
                //update stock
                if (returnQty > stock.InQty) throw new BadRequestException($"Return quantity {returnQty} is greater than stock quantity {stock.InQty} !!");
                if (returnQty > stock.AvailableQty) throw new BadRequestException($"Return quantity {returnQty} exceeds available stock quantity {stock.AvailableQty}");
                stock.AvailableQty = stock.AvailableQty - returnQty;
                await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
                //return stock
                await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                {
                    Id = Guid.NewGuid(),
                    TransactonType = (int)TransactonType.PurchaseReturn,
                    InventoryTypeId = product.InventoryTypeId,
                    ProductTypeId = product.ProductTypeId,
                    ProductId = stock.ProductId,
                    StoreId = stock.StoreId,
                    OutQty = returnQty,
                    OutRate = stock.InRate + stock.InTransportCost,
                    BatchNo = stock.BatchNo,
                    TransactionId = purchaseReturn.Id,
                    StockInDate = stock.StockInDate,
                    TransactionDate = purchaseReturn.PurchaseReturnDate,
                    Remark = "Purchase Return Against " + purchaseReturn?.Grnno,
                    FinancialYearId = financialYear.Id
                });

                return returnQty * (stock.InRate + stock.InTransportCost);
            }

        }

        private async Task ReverseStock(PurchaseReturn purchaseReturn)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.PurchaseReturn
                      && x.TransactionId == purchaseReturn.Id).ToListAsync();
            if (stocks.Any())
            {
                foreach (var stock in stocks)
                {
                    await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                    {
                        Id = Guid.NewGuid(),
                        TransactonType = (int)TransactonType.Purchase,
                        InventoryTypeId = stock.InventoryTypeId,
                        ProductTypeId = stock.ProductTypeId,
                        ProductId = stock.ProductId,
                        StoreId = stock.StoreId,
                        AvailableQty = stock.OutQty,
                        InQty = stock.OutQty,
                        InRate = stock.OutRate,
                        BatchNo = stock.BatchNo,
                        TransactionId = purchaseReturn!.Id,
                        StockInDate = stock.StockInDate,
                        TransactionDate = DateTime.Now,
                        Remark = "Reverse Stock for Purchase Return#" + purchaseReturn?.PurchaseReturnNo + " Unposted",
                        IsUnpostedEntry = true,
                        FinancialYearId = financialYear.Id
                    });
                }
                await _unitOfWork.Repository<Stock>().DeleteAsync(stocks);
            }
        }

        private async Task InsertTransaction(PurchaseReturn purchaseReturn, decimal cogsTotal)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append("Purchase Return-");
            var transactionQty = 0;
            var transactionQtyValue = 0m;
            foreach (var item in purchaseReturn!.PurchaseReturnDetails)
            {
                transactionQty += item.Quantity;
                transactionQtyValue += (item.Quantity * item.Rate);
                sb.Append(" ");
                sb.Append(item?.Product?.Name);
                sb.Append(", ");
                sb.Append(item?.Quantity);
                sb.Append(" ");
                sb.Append(item?.Product?.MeasurementUnit?.Name);
                sb.Append(" @");
                sb.Append(item?.Rate);
            }

            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            var supplierAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == purchaseReturn.SupplierId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(supplierAccountName);
            contraAccountIdsForCredit.Append(purchaseReturn.SupplierId.ToString());
            var purchaseReturnClearingAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchaseReturnClearingAccount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(", " + purchaseReturnClearingAccountName);
            contraAccountIdsForCredit.Append(", " + AccountHeadConstants.PurchaseReturnClearingAccount);

            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var rmInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.RawMaterialsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(rmInventoryAccountName);
            contraAccountIdsForDebit.Append(AccountHeadConstants.RawMaterialsInventory);
            contraAccountNamesForDebit.Append(", " + purchaseReturnClearingAccountName);
            contraAccountIdsForDebit.Append(", " + AccountHeadConstants.PurchaseReturnClearingAccount);
            var inventoryAdjustAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.InventoryAdjust.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            var adjustmentAmount = cogsTotal - purchaseReturn.TotalAmount;
            if (adjustmentAmount > 0m)
            {
                contraAccountNamesForCredit.Append(", " + inventoryAdjustAccountName);
                contraAccountIdsForCredit.Append(", " + AccountHeadConstants.InventoryAdjust);
                contraAccountNamesForDebit.Append(", " + rmInventoryAccountName);
                contraAccountIdsForDebit.Append(", " + AccountHeadConstants.RawMaterialsInventory);
            }
            if (adjustmentAmount < 0m)
            {
                contraAccountNamesForCredit.Append(", " + rmInventoryAccountName);
                contraAccountIdsForCredit.Append(", " + AccountHeadConstants.RawMaterialsInventory);
                contraAccountNamesForDebit.Append(", " + inventoryAdjustAccountName);
                contraAccountIdsForDebit.Append(", " + AccountHeadConstants.InventoryAdjust);
            }
            //Transaction Entries start here
            //party a/c debit
            await _accountService.HitAccount(_unitOfWork, null, purchaseReturn.PurchaseReturnNo, sb.ToString(),
            purchaseReturn.SupplierId, purchaseReturn.TotalAmount, 0, purchaseReturn.PurchaseReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString(), (int)AccountTransactonType.PurchaseReturn, -transactionQty, -transactionQtyValue);
            //inventory a/c credit
            await _accountService.HitAccount(_unitOfWork, null, purchaseReturn.PurchaseReturnNo, sb.ToString(),
                  Guid.Parse(AccountHeadConstants.RawMaterialsInventory), 0, purchaseReturn.TotalAmount, purchaseReturn.PurchaseReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            //purchase return clearing a/c debit
            await _accountService.HitAccount(_unitOfWork, null, purchaseReturn.PurchaseReturnNo, sb.ToString(),
                  Guid.Parse(AccountHeadConstants.PurchaseReturnClearingAccount), purchaseReturn.TotalAmount, 0, purchaseReturn.PurchaseReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            //purchase return clearing a/c credit
            await _accountService.HitAccount(_unitOfWork, null, purchaseReturn.PurchaseReturnNo, sb.ToString(),
                  Guid.Parse(AccountHeadConstants.PurchaseReturnClearingAccount), 0, purchaseReturn.TotalAmount, purchaseReturn.PurchaseReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

            //damage lost when stock value is greater than purchase return value
            if (adjustmentAmount == 0m) return;
            if (adjustmentAmount > 0m)
            {
                //inventory a/c credit 
                await _accountService.HitAccount(_unitOfWork, null, purchaseReturn.PurchaseReturnNo, sb.ToString(),
                     Guid.Parse(AccountHeadConstants.RawMaterialsInventory), 0, adjustmentAmount, purchaseReturn.PurchaseReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

                //Damage Lost Inventory account debit
                await _accountService.HitAccount(_unitOfWork, null, purchaseReturn.PurchaseReturnNo, sb.ToString(),
                          Guid.Parse(AccountHeadConstants.InventoryAdjust), adjustmentAmount, 0, purchaseReturn.PurchaseReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            }
            else
            {
                //inventory a/c debit 
                await _accountService.HitAccount(_unitOfWork, null, purchaseReturn.PurchaseReturnNo, sb.ToString(),
                     Guid.Parse(AccountHeadConstants.RawMaterialsInventory), Math.Abs(adjustmentAmount), 0, purchaseReturn.PurchaseReturnDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

                //Damage Lost Inventory account credit
                await _accountService.HitAccount(_unitOfWork, null, purchaseReturn.PurchaseReturnNo, sb.ToString(),
                          Guid.Parse(AccountHeadConstants.InventoryAdjust), 0, Math.Abs(adjustmentAmount), purchaseReturn.PurchaseReturnDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            }
        }

        private async Task DeleteTransaction(string purchaseReturnNo)
        {
            var purchaseReturn = await _unitOfWork.Repository<PurchaseReturn>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.PurchaseReturnNo == purchaseReturnNo);
            if (purchaseReturn is null) throw new NotFoundResultException("Purchase Return Not Found With this Purchase Return No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == purchaseReturn.PurchaseReturnNo);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }

        }

        private async Task InsertSupplierTransactionAgainstPO(PurchaseReturn purchaseReturn)
        {
            if (!string.IsNullOrWhiteSpace(purchaseReturn.Grnno))
            {
                var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking().SingleOrDefaultAsync(x => x.Grnno == purchaseReturn.Grnno);
                if (!string.IsNullOrWhiteSpace(goodsReceiveNote!.Ponumber))
                {
                    var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().SingleOrDefaultAsync(x => x.Ponumber == goodsReceiveNote.Ponumber);
                    await _unitOfWork.Repository<SupplierTransactionAgainstPo>().AddAsync(new SupplierTransactionAgainstPo
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = purchaseReturn.Id,
                        TransactionDate = purchaseReturn.PurchaseReturnDate,
                        SupplierInvoiceDate = null,
                        PaymentTermInDays = 0,
                        PurchaseOrderId = purchaseOrder!.Id,
                        SupplierId = purchaseReturn.SupplierId,
                        SupplierTransactionType = (int)SupplierTransactionType.Purchase_Return,
                        Amount = -purchaseReturn.TotalAmount,
                        Remark = "Purchase Return"
                    });
                }

            }
        }

        private async Task RemoveSupplierTransactionAgainstPO(PurchaseReturn purchaseReturn)
        {
            var transactions = _unitOfWork.Repository<SupplierTransactionAgainstPo>().TableNoTracking().Where(x => x.TransactionId == purchaseReturn.Id);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<SupplierTransactionAgainstPo>().DeleteAsync(transaction.Id);
                }
            }
        }


    }
}
