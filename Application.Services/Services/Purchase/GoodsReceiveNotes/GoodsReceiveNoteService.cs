using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Purchase.GoodsReceiveNote;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.SearchRequestModels.Report;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.GoodsReceiveNote;
using Application.Services.ViewModels.Report;
using Application.Services.ViewModels.Report.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Slicer.Style;
using System.Linq;
using System.Text;
using static iTextSharp.text.pdf.AcroFields;

namespace Application.Services.Services.Purchase.GoodsReceiveNotes
{
    public class GoodsReceiveNoteService : BaseService<GoodsReceiveNote, GoodsReceiveNoteCreationDto, GoodsReceiveNoteUpdateDto, GoodsReceiveNoteRequestModel, GoodsReceiveNoteViewModel>, IGoodsReceiveNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IAccountService _accountService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;

        public GoodsReceiveNoteService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, IAccountService accountService
            , INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
        }


        public async Task<GoodsReceiveNoteAggregatorModel> PrepareGoodsReceiveNoteAggregatorModel(GoodsReceiveNoteRequestModel goodsReceiveNoteRequest)
        {
            var goodsReceiveNoteQueryable = _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking().Where(goodsReceiveNoteRequest.GetExpression());
            return new GoodsReceiveNoteAggregatorModel
            {
                AggregatorSubtotal = await goodsReceiveNoteQueryable.SumAsync(x => x.Subtotal),
                AggregatorTransportationCost = await goodsReceiveNoteQueryable.SumAsync(x => x.TransportationCost),
                AggregatorTotal = await goodsReceiveNoteQueryable.SumAsync(x => x.Total)
            };
        }

        public async Task<GoodsReceiveNoteViewModel> GetByIdAsync(Guid id)
        {
            var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Supplier).Include(x => x.Store)
                .Include(x => x.GoodsReceiveNoteDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (goodsReceiveNote == null) throw new NotFoundResultException("Goods Receive Note Not Found With this Id");
            return _mapper.Map<GoodsReceiveNoteViewModel>(goodsReceiveNote);
        }

        public new async Task<Guid> AddAsync(GoodsReceiveNoteCreationDto goodsReceiveNoteCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            if (goodsReceiveNoteCreationDto.IsImportPurchase)
            {
                var purchaseInvoiceExistsAgainstPO = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().AnyAsync(x => x.Ponumber.Contains(goodsReceiveNoteCreationDto.Ponumber!));
                if (purchaseInvoiceExistsAgainstPO) throw new BadRequestException("You cannot create another GRN for PO #" + goodsReceiveNoteCreationDto.Ponumber + " as it is already invoiced.");
            }
            goodsReceiveNoteCreationDto.Grndate = goodsReceiveNoteCreationDto.Grndate.ToLocal();
            var goodsReceiveNote = _mapper.Map<GoodsReceiveNote>(goodsReceiveNoteCreationDto);
            var count = _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            goodsReceiveNote.Id = Guid.NewGuid();
            goodsReceiveNote.Grnno = "GRN" + financialYear.Code + "-" + count.ToString().PadLeft(6, '0');
            goodsReceiveNote.FinancialYearId = financialYear.Id;
            goodsReceiveNote.Status = (int)GRNStatus.Pending;
            foreach (var item in goodsReceiveNote.GoodsReceiveNoteDetails)
            {
                if (item.Grnquantity == 0 || item.NetQuantity == 0) continue;
                item.Id = Guid.NewGuid();
                item.GoodsReceiveNoteId = goodsReceiveNote.Id;
                item.ExpiryDate = item.ExpiryDate?.ToLocal();
                await _unitOfWork.Repository<GoodsReceiveNoteDetail>().AddAsync(item);

            }
            goodsReceiveNote.GoodsReceiveNoteDetails = null;
            await _unitOfWork.Repository<GoodsReceiveNote>().AddAsync(goodsReceiveNote);

            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, goodsReceiveNote.Id
                , "/purchase/goods-receive-note/" + goodsReceiveNote.Id, Permissions.GoodsReceiveNotes.Check,
                "Goods Receive Note " + goodsReceiveNote.Grnno + " is ready for Approval", (int)GRNStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            return goodsReceiveNote.Id;
        }

        public new async Task<Guid> UpdateAsync(GoodsReceiveNoteUpdateDto goodsReceiveNoteUpdateDto)
        {
            var dbGoodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().FindAsync(goodsReceiveNoteUpdateDto.Id);
            //when date updated,convert it to local date
            if (goodsReceiveNoteUpdateDto.Grndate.Equals(dbGoodsReceiveNote.Grndate) == false)
            {
                goodsReceiveNoteUpdateDto.Grndate = goodsReceiveNoteUpdateDto.Grndate.ToLocal();
            }
            if (dbGoodsReceiveNote == null) throw new NotFoundResultException("GoodsReceiveNote Not Found With this Id");
            var goodsReceiveNote = _mapper.Map(goodsReceiveNoteUpdateDto, dbGoodsReceiveNote);
            if (dbGoodsReceiveNote.Status == (int)GRNStatus.Pending || dbGoodsReceiveNote.Status == (int)GRNStatus.Checked)
            {
                await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(goodsReceiveNote);
                foreach (var item in goodsReceiveNote.GoodsReceiveNoteDetails)
                {

                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.GoodsReceiveNoteId = goodsReceiveNote.Id;
                        await _unitOfWork.Repository<GoodsReceiveNoteDetail>().AddAsync(item);
                    }
                    else
                    {
                        var dbGoodsReceiveNoteDetails = await _unitOfWork.Repository<GoodsReceiveNoteDetail>().FindAsync(x => x.Id == item.Id);
                        if (item.ExpiryDate.Equals(dbGoodsReceiveNoteDetails?.ExpiryDate) == false)
                        {
                            item.ExpiryDate = item.ExpiryDate?.ToLocal();
                        }
                        await _unitOfWork.Repository<GoodsReceiveNoteDetail>().UpdateAsync(item);
                    }
                }
                //Soft Delete for DeletedGoodsReceiveNoteDetailIds
                if (!string.IsNullOrEmpty(goodsReceiveNoteUpdateDto.DeletedGoodsReceiveNoteDetailIds))
                {
                    foreach (var id in goodsReceiveNoteUpdateDto.DeletedGoodsReceiveNoteDetailIds.Split(',').Where(x => x != ""))
                    {
                        Guid goodReceiveNoteDetailId = Guid.Parse(id);
                        var dbGoodsReceiveNoteDetail = await _unitOfWork.Repository<GoodsReceiveNoteDetail>().FindAsync(goodReceiveNoteDetailId);
                        dbGoodsReceiveNoteDetail.Deleted = true;
                        await _unitOfWork.Repository<GoodsReceiveNoteDetail>().UpdateAsync(dbGoodsReceiveNoteDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");

            return goodsReceiveNote.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().FindAsync(x => x.Id == id, x => x.Include(x => x.GoodsReceiveNoteDetails));
            if (goodsReceiveNote == null) throw new NotFoundResultException("GoodsReceiveNote Not Found With this Id");
            if (goodsReceiveNote.Status == (int)GRNStatus.Pending)
            {
                foreach (var item in goodsReceiveNote.GoodsReceiveNoteDetails)
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<GoodsReceiveNoteDetail>().UpdateAsync(item);
                }
                goodsReceiveNote.Deleted = true;
                await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(goodsReceiveNote);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");

            return goodsReceiveNote.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var goodsReceiveNotes = _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await goodsReceiveNotes.Where(x => x.Status == (int)GRNStatus.Pending).CountAsync(),
                CheckedCount = await goodsReceiveNotes.Where(x => x.Status == (int)GRNStatus.Checked).CountAsync()
            };
            return response;

        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbGoodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().FindAsync(id);
            if (dbGoodsReceiveNote.Status == (int)GRNStatus.Pending)
            {
                dbGoodsReceiveNote.Status = (int)GRNStatus.Checked;
                dbGoodsReceiveNote.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(dbGoodsReceiveNote);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbGoodsReceiveNote.Id
                       && x.Status == (int)GRNStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbGoodsReceiveNote.Id
                    , "/purchase/goods-receive-note/" + dbGoodsReceiveNote.Id, Permissions.PurchaseRequisitions.Approve,
                    "Goods Receive Note " + dbGoodsReceiveNote.Grnno + " is ready for Approval", (int)GRNStatus.Checked);


                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("GRN Status has already been checked by " + dbGoodsReceiveNote?.CheckedBy);

        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<GoodsReceiveNote>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy, x.Ponumber, x.Grnno })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Goods Receive Note not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Goods Receive Note does not belong to the current financial year.");

            var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().Include(x => x.PurchaseOrderDetails).SingleOrDefaultAsync(x => x.Ponumber == dbInfo.Ponumber);
            if (purchaseOrder is null) throw new NotFoundResultException("Purchase Order not found associated with this Goods Receive Note " + dbInfo.Grnno);

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<GoodsReceiveNote>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)GRNStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)GRNStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"GRN already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbGoodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.Store).Include(x => x.GoodsReceiveNoteDetails).ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));

            dbGoodsReceiveNote.Status = (int)GRNStatus.Approved;
            dbGoodsReceiveNote.ApprovedBy = approvedBy;

            #region Increase Received Quantity, update product last purchase Rate,Insert Stock,change po status and insert transaction
            int totalReceivedQuantity = purchaseOrder.PurchaseOrderDetails.Sum(x => x.ReceivedQuantity);
            decimal totalAdditionalLandedCost = 0m;
            foreach (var item in dbGoodsReceiveNote.GoodsReceiveNoteDetails)
            {
                var additionalCostPerQuantity = 0m;
                if (purchaseOrder.IsImportPurchase)
                {
                    additionalCostPerQuantity = (item.RateAfterBagWeightDeduction * purchaseOrder.AdditionalLandedCost) / purchaseOrder.Subtotal;
                    totalAdditionalLandedCost += (item.NetQuantity * additionalCostPerQuantity);
                }
                else
                {
                    additionalCostPerQuantity = (item.RateAfterBagWeightDeduction * dbGoodsReceiveNote.TransportationCost) / dbGoodsReceiveNote.Subtotal;
                }
                await IncreasePurchaseOrderDetailReceivedQuantityByGRNQuantity(item);
                totalReceivedQuantity += item.Grnquantity;
                var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
                product.LastPurchaseRate = item.Rate;
                await _unitOfWork.Repository<Product>().UpdateAsync(product);
                await InsertStock(dbGoodsReceiveNote, additionalCostPerQuantity, item, product);
            }
            if (purchaseOrder.IsImportPurchase)
            {
                dbGoodsReceiveNote.AdditionalLandedCost = totalAdditionalLandedCost;
                dbGoodsReceiveNote.Total += totalAdditionalLandedCost;
            }
            await ChangePurchaseOrderStatus(dbGoodsReceiveNote, purchaseOrder, totalReceivedQuantity);
            await InsertTransaction(dbGoodsReceiveNote, purchaseOrder);
            #endregion
            foreach (var item in dbGoodsReceiveNote.GoodsReceiveNoteDetails)
            {
                item.Product = null;
            }

            await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(dbGoodsReceiveNote);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbGoodsReceiveNote.Id
                   && x.Status == (int)GRNStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }
        private async Task<bool> IsDuplicateRequest(Guid goodsReceiveNoteId)
        {
            var stockInserted = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .AnyAsync(x => x.TransactonType == (int)TransactonType.Purchase
                       && x.TransactionId == goodsReceiveNoteId);
            return stockInserted;
        }
        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Load minimal data (no tracking)
            var dbInfo = await _unitOfWork.Repository<GoodsReceiveNote>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.Ponumber, x.Grnno })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("GoodsReceiveNote Not Found With this Id");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Goods Receive Note does not belong to the current financial year.");
            if (dbInfo.Status != fromStatus) throw new BadRequestException("Goods Receive Note status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)GRNStatus.Checked && dbInfo.Status != (int)GRNStatus.Approved)
                throw new BadRequestException("Only Checked or Approved Goods Receive Note can be unposted.");

            var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().Include(x => x.PurchaseOrderDetails).SingleOrDefaultAsync(x => x.Ponumber == dbInfo.Ponumber);
            if (purchaseOrder is null) throw new NotFoundResultException("Purchase Order not found associated with this Goods Receive Note " + dbInfo.Grnno);

            var newStatus = dbInfo.Status == (int)GRNStatus.Approved
                ? (int)GRNStatus.Checked
                : (int)GRNStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<GoodsReceiveNote>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Goods Receive Note status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.GoodsReceiveNoteDetails));

            goodsReceiveNote.Status = newStatus;
            goodsReceiveNote.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)GRNStatus.Checked:
                    goodsReceiveNote.CheckedBy = "";
                    break;
                case (int)GRNStatus.Approved:
                    goodsReceiveNote.ApprovedBy = "";
                    #region Delete Related Entity
                    await DeleteTransaction(goodsReceiveNote.Grnno);
                    await DeleteStock(goodsReceiveNote.Id);
                    int totalReceivedQuantity = purchaseOrder.PurchaseOrderDetails.Sum(x => x.ReceivedQuantity);
                    foreach (var item in goodsReceiveNote.GoodsReceiveNoteDetails)
                    {
                        await ReducePurchaseOrderDetailReceivedQuantityByGRNQuantity(item);
                        totalReceivedQuantity -= item.Grnquantity;
                    }
                    await ChangePurchaseOrderStatus(goodsReceiveNote, purchaseOrder, totalReceivedQuantity);
                    if (purchaseOrder.IsImportPurchase)
                    {
                        goodsReceiveNote.Total -= goodsReceiveNote.AdditionalLandedCost;
                        goodsReceiveNote.AdditionalLandedCost = 0;
                    }
                    #endregion
                    break;
            }

            await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(goodsReceiveNote);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return goodsReceiveNote.Status;
        }

        private async Task ChangePurchaseOrderStatus(GoodsReceiveNote goodsReceiveNote, PurchaseOrder purchaseOrder, int totalReceivedQuantity)
        {
            if (!string.IsNullOrWhiteSpace(goodsReceiveNote.Ponumber))
            {
                var sumPoQuantity = purchaseOrder!.PurchaseOrderDetails.Sum(x => x.Quantity);
                decimal weightVariance = Math.Abs(purchaseOrder!.WeightVariance);
                decimal weightVarianceQty = (weightVariance / 100) * sumPoQuantity;//such as 5% of ordered qty
                if (totalReceivedQuantity > (sumPoQuantity + weightVarianceQty)) throw new BadRequestException("Received Qty exceeded the Ordered Qty(including weight variance)!!");
                if (totalReceivedQuantity >= sumPoQuantity)
                    purchaseOrder.Status = (int)PurchaseOrderStatus.Item_Completely_Received;
                else if (totalReceivedQuantity == 0)
                {
                    if (purchaseOrder.IsImportPurchase) purchaseOrder.Status = (int)PurchaseOrderStatus.Ready_For_GRN;
                    else purchaseOrder.Status = (int)PurchaseOrderStatus.Approved;
                }
                else purchaseOrder.Status = (int)PurchaseOrderStatus.Item_Partially_Received;
                purchaseOrder.PurchaseOrderDetails = null;
                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(purchaseOrder);
            }
        }

        private async Task InsertStock(GoodsReceiveNote goodsReceiveNote, decimal additionalCostPerQuantity, GoodsReceiveNoteDetail item, Product product)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.Purchase,
                InventoryTypeId = product.InventoryTypeId,
                ProductTypeId = product.ProductTypeId,
                ProductId = item.ProductId,
                SupplierId = goodsReceiveNote.SupplierId,
                StoreId = goodsReceiveNote.StoreId,
                AvailableQty = item.NetQuantity,
                InQty = item.NetQuantity,
                InRate = item.RateAfterBagWeightDeduction,
                InTransportCost = additionalCostPerQuantity,
                BatchNo = goodsReceiveNote.Grnno,
                TransactionId = goodsReceiveNote.Id,
                StockInDate = goodsReceiveNote.Grndate,
                TransactionDate = goodsReceiveNote.Grndate,
                Remark = goodsReceiveNote?.Grnno + ", " + goodsReceiveNote?.Supplier?.Name + " TR#" + goodsReceiveNote?.TruckNo,
                FinancialYearId = financialYear.Id
            });
        }

        private async Task DeleteStock(Guid goodsReceiveNoteId)
        {
            var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.Purchase
                       && x.TransactionId == goodsReceiveNoteId).ToListAsync();
            foreach (var stock in stocks)
            {
                if (stock.AvailableQty == stock.InQty)
                {
                    await _unitOfWork.Repository<Stock>().DeleteAsync(stock.Id);
                }
                else throw new BadRequestException("Stock Already Occupied ! Not Possible to Unpost");
            }
        }

        private async Task InsertTransaction(GoodsReceiveNote goodsReceiveNote, PurchaseOrder purchaseOrder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(goodsReceiveNote?.Ponumber);
            foreach (var item in goodsReceiveNote!.GoodsReceiveNoteDetails)
            {
                sb.Append(" ");
                sb.Append(item?.Product?.Name);
                sb.Append(", GRN: ");
                sb.Append(item?.Grnquantity);
                sb.Append(" ");
                sb.Append(item?.Product?.MeasurementUnit?.Name);
                sb.Append(" @");
                sb.Append(item?.Rate);
                sb.Append(", NET: ");
                sb.Append(item?.NetQuantity);
                sb.Append(" ");
                sb.Append(item?.Product?.MeasurementUnit?.Name);
                sb.Append(" @");
                sb.Append(item?.RateAfterBagWeightDeduction);
            }

            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            if (goodsReceiveNote.Store.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid())
            {
                var rmName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.RawMaterialsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForCredit.Append(rmName);
                contraAccountIdsForCredit.Append(AccountHeadConstants.RawMaterialsInventory);
            }
            else
            {
                var fgName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.FinishedGoodsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForCredit.Append(fgName);
                contraAccountIdsForCredit.Append(AccountHeadConstants.FinishedGoodsInventory);
            }
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var purchaseClearingAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchaseClearingAccount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(purchaseClearingAccountName);
            contraAccountIdsForDebit.Append(AccountHeadConstants.PurchaseClearingAccount);

            if (purchaseOrder.IsImportPurchase)
            {
                if ((goodsReceiveNote.Subtotal + goodsReceiveNote.AdditionalLandedCost) == goodsReceiveNote.Total)
                {
                    var lccostClearingAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.LCCostClearingAccount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", ");
                    contraAccountNamesForDebit.Append(lccostClearingAccountName);
                    contraAccountIdsForDebit.Append(", ");
                    contraAccountIdsForDebit.Append(AccountHeadConstants.LCCostClearingAccount);
                    //purchase account credit
                    await _accountService.HitAccount(_unitOfWork, null, goodsReceiveNote.Grnno, sb.ToString(),
                     Guid.Parse(AccountHeadConstants.PurchaseClearingAccount), 0, goodsReceiveNote.Subtotal, goodsReceiveNote.Grndate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                    //Lc Cost Clearing Account credit
                    await _accountService.HitAccount(_unitOfWork, null, goodsReceiveNote.Grnno, sb.ToString(),
                      Guid.Parse(AccountHeadConstants.LCCostClearingAccount), 0, goodsReceiveNote.AdditionalLandedCost, goodsReceiveNote.Grndate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                    if (goodsReceiveNote.Store.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid())
                    {
                        //RM Inventory account debit
                        await _accountService.HitAccount(_unitOfWork, null, goodsReceiveNote.Grnno, sb.ToString(),
                            Guid.Parse(AccountHeadConstants.RawMaterialsInventory), goodsReceiveNote.Total, 0, goodsReceiveNote.Grndate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                    }
                    else
                    {
                        //FinishedGoods Inventory account debit
                        await _accountService.HitAccount(_unitOfWork, null, goodsReceiveNote.Grnno, sb.ToString(),
                            Guid.Parse(AccountHeadConstants.FinishedGoodsInventory), goodsReceiveNote.Total, 0, goodsReceiveNote.Grndate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                    }
                }
                else throw new BadRequestException("Debit & Credit are not equal");

            }
            else
            {
                if ((goodsReceiveNote.Subtotal + goodsReceiveNote.TransportationCost) == goodsReceiveNote.Total)
                {
                    if (goodsReceiveNote.TransportationCost != 0m)
                    {
                        var carryingPayableAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.CarryingPayable.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                        contraAccountNamesForDebit.Append(", ");
                        contraAccountNamesForDebit.Append(carryingPayableAccountName);
                        contraAccountIdsForDebit.Append(", ");
                        contraAccountIdsForDebit.Append(AccountHeadConstants.CarryingPayable);
                    }
                    //purchase account credit
                    await _accountService.HitAccount(_unitOfWork, null, goodsReceiveNote.Grnno, sb.ToString(),
                     Guid.Parse(AccountHeadConstants.PurchaseClearingAccount), 0, goodsReceiveNote.Subtotal, goodsReceiveNote.Grndate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                    //carriage inward Account credit
                    if (goodsReceiveNote.TransportationCost != 0m)
                    {
                        await _accountService.HitAccount(_unitOfWork, null, goodsReceiveNote.Grnno, sb.ToString(),
                          Guid.Parse(AccountHeadConstants.CarryingPayable), 0, goodsReceiveNote.TransportationCost, goodsReceiveNote.Grndate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                    }

                    if (goodsReceiveNote.Store.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid())
                    {
                        //RM Inventory account debit
                        await _accountService.HitAccount(_unitOfWork, null, goodsReceiveNote.Grnno, sb.ToString(),
                            Guid.Parse(AccountHeadConstants.RawMaterialsInventory), goodsReceiveNote.Total, 0, goodsReceiveNote.Grndate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                    }
                    else
                    {
                        //FinishedGoods Inventory account debit
                        await _accountService.HitAccount(_unitOfWork, null, goodsReceiveNote.Grnno, sb.ToString(),
                            Guid.Parse(AccountHeadConstants.FinishedGoodsInventory), goodsReceiveNote.Total, 0, goodsReceiveNote.Grndate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                    }
                }
                else throw new BadRequestException("Debit & Credit are not equal");
            }

        }

        private async Task DeleteTransaction(string grnNo)
        {
            var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Grnno == grnNo);
            if (goodsReceiveNote is null) throw new NotFoundResultException("GoodsReceiveNote Not Found With this Grn No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == goodsReceiveNote.Grnno);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }

        }

        private async Task ReducePurchaseOrderDetailReceivedQuantityByGRNQuantity(GoodsReceiveNoteDetail goodsReceiveNoteDetail)
        {
            if (goodsReceiveNoteDetail.PurchaseOrderDetailId.HasValue)
            {
                var purcahseOrderDetail = await _unitOfWork.Repository<PurchaseOrderDetail>().FindAsync(goodsReceiveNoteDetail.PurchaseOrderDetailId.Value);
                purcahseOrderDetail.ReceivedQuantity -= goodsReceiveNoteDetail.Grnquantity;
                await _unitOfWork.Repository<PurchaseOrderDetail>().UpdateAsync(purcahseOrderDetail);
            }
        }

        private async Task IncreasePurchaseOrderDetailReceivedQuantityByGRNQuantity(GoodsReceiveNoteDetail goodsReceiveNoteDetail)
        {
            if (goodsReceiveNoteDetail.PurchaseOrderDetailId.HasValue)
            {
                var purcahseOrderDetail = await _unitOfWork.Repository<PurchaseOrderDetail>().FindAsync(goodsReceiveNoteDetail.PurchaseOrderDetailId.Value);
                purcahseOrderDetail.ReceivedQuantity += goodsReceiveNoteDetail.Grnquantity;
                await _unitOfWork.Repository<PurchaseOrderDetail>().UpdateAsync(purcahseOrderDetail);
            }
        }

        public async Task<List<GrnItemViewModel>> GrnSupplierItemWiseSearchAsync(GoodsReceiveNoteRequestModel request)
        {
            var queryable = _unitOfWork.Repository<GoodsReceiveNoteDetail>().TableNoTracking()
                .Include(x => x.GoodsReceiveNote).ThenInclude(x => x.Store).AsQueryable();
            queryable = queryable.Where(x => x.GoodsReceiveNote.Status >= (int)GRNStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                queryable = queryable.Where(d => d.GoodsReceiveNote.Grndate.Date >= request.FromDate.Value.Date && d.GoodsReceiveNote.Grndate.Date <= request.ToDate.Value.Date);
            }
            if (request.SupplierId.HasValue)
            {
                queryable = queryable.Where(d => d.GoodsReceiveNote.SupplierId == request.SupplierId.Value);
            }
            if (request.ProductId.HasValue)
            {
                queryable = queryable.Where(d => d.ProductId == request.ProductId.Value);
            }
            if (request.StoreId.HasValue)
            {
                queryable = queryable.Where(d => d.GoodsReceiveNote.StoreId == request.StoreId.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.Ponumber))
            {
                queryable = queryable.Where(d => d.GoodsReceiveNote.Ponumber == request.Ponumber);
            }
            var goodsReceiveNoteItems = await queryable.Select(d => new GrnItemViewModel
            {
                Id = d.Id,
                GrnNo = d.GoodsReceiveNote.Grnno,
                GrnDate = d.GoodsReceiveNote.Grndate,
                SupplierName = d.GoodsReceiveNote.Supplier.Name,
                StoreName = d.GoodsReceiveNote.Store.Name,
                ProductName = d.Product.Name,
                Description = d.GoodsReceiveNote.ChallanNo + ", " + d.GoodsReceiveNote.TruckNo,
                Pono = d.GoodsReceiveNote.Ponumber,
                GrnQuantity = d.Grnquantity,
                Rate = d.Rate,
                Amount = d.Amount,
                TransportationRate = (d.Rate * d.GoodsReceiveNote.TransportationCost) / d.GoodsReceiveNote.Subtotal,
                AdditionalLandedCostRate = (d.Rate * d.GoodsReceiveNote.AdditionalLandedCost) / d.GoodsReceiveNote.Subtotal
            }).OrderBy(x => x.GrnDate).ToListAsync();

            return goodsReceiveNoteItems;
        }

        public async Task<List<GrnItemSummaryViewModel>> GrnItemSearchAsync(GoodsReceiveNoteRequestModel request)
        {
            var queryable = _unitOfWork.Repository<GoodsReceiveNoteDetail>().TableNoTracking()
                .Include(x => x.GoodsReceiveNote).AsQueryable();
            queryable = queryable.Where(x => x.GoodsReceiveNote.Status >= (int)GRNStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                queryable = queryable.Where(d => d.GoodsReceiveNote.Grndate.Date >= request.FromDate.Value.Date && d.GoodsReceiveNote.Grndate.Date <= request.ToDate.Value.Date);
            }
            var grnItems = await queryable.GroupBy(x => x.ProductId).Select(d => new GrnItemSummaryViewModel
            {
                Id = d.Key,
                Code = d.First().Product.Code,
                ProductName = d.First().Product.Name,
                ProductTypeName = d.First().Product.ProductType.Name,
                GrnQuantity = d.Sum(x => x.Grnquantity),
                Rate = d.Sum(x => x.Amount) / d.Sum(x => x.Grnquantity),
                Amount = d.Sum(x => x.Amount),
                TransportationCost = d.Sum(x => ((x.Rate * x.GoodsReceiveNote.TransportationCost) / x.GoodsReceiveNote.Subtotal) * x.Grnquantity),
                TransportationRate = d.Sum(x => ((x.Rate * x.GoodsReceiveNote.TransportationCost) / x.GoodsReceiveNote.Subtotal) * x.Grnquantity) / d.Sum(x => x.Grnquantity)
            }).OrderBy(x => x.ProductName).ToListAsync();

            return grnItems;
        }

        public async Task<List<ProductWithLatestPriceViewModel>> GetProductsWithLatestPurchasePriceAsync(ProductWithLatestPriceRequestModel request)
        {
            var grn = _unitOfWork.Repository<GoodsReceiveNoteDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.ProductType).Include(x => x.Product).ThenInclude(x => x.MeasurementUnit)
                .Include(x => x.GoodsReceiveNote).Where(x => x.GoodsReceiveNote.Status >= (int)GRNStatus.Approved);

            var inventoryTypeId = InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid();
            var rmStock = _unitOfWork.Repository<Stock>().TableNoTracking()
                .Where(x => !x.IsUnpostedEntry && x.InventoryTypeId == inventoryTypeId && x.TransactionDate.Date <= request.ToDate!.Value.Date);

            if (request.ToDate.HasValue)
            {
                var toDate = request.ToDate.Value.Date;
                grn = grn.Where(x => x.GoodsReceiveNote.Grndate < toDate);
            }

            if (request.ProductId.HasValue)
            {
                grn = grn.Where(x => x.ProductId == request.ProductId.Value);
                rmStock = rmStock.Where(x => x.ProductId == request.ProductId.Value);
            }

            if (request.ProductTypeId.HasValue)
            {
                grn = grn.Where(x => x.Product.ProductTypeId == request.ProductTypeId.Value);
                rmStock = rmStock.Where(x => x.Product.ProductTypeId == request.ProductTypeId.Value);
            }


            // latest GRN date per product
            var latestDate = await grn.GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    LastDate = g.Max(x => x.GoodsReceiveNote.Grndate)
                }).ToListAsync();


            var latestDateDict = latestDate.ToDictionary(x => x.ProductId, x => x.LastDate);


            // latest rows
            var grnList = await grn.ToListAsync();
            var lastRows = grnList.Where(x => latestDateDict.ContainsKey(x.ProductId) && x.GoodsReceiveNote.Grndate == latestDateDict[x.ProductId]).ToList();

            // STOCK QUERY
            var stockResult = await rmStock.GroupBy(x => x.ProductId)
                                .Select(g => new
                                {
                                    ProductId = g.Key,
                                    Qty = g.Sum(x => x.InQty - x.OutQty)
                                }).ToListAsync();
            var stockDict = stockResult.ToDictionary(x => x.ProductId, x => x.Qty);

            var grnResult = lastRows
                .GroupBy(x => x.ProductId)
                .Select(g => new ProductWithLatestPriceViewModel
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    ProductTypeName = g.First().Product.ProductType.Name,
                    MeasurementUnitName = g.First().Product.MeasurementUnit.Name,
                    GrnDate = g.First().GoodsReceiveNote.Grndate,
                    GrnQuantity = g.Sum(x => x.Grnquantity),
                    GrnRate = g.OrderByDescending(x => x.CreatedOn).First().Rate,
                    LatestRate = g.OrderByDescending(x => x.CreatedOn).First().RateAfterBagWeightDeduction,
                    BalanceQuantity = stockDict.ContainsKey(g.Key) ? stockDict[g.Key] : 0
                }).OrderBy(x => x.ProductName).ToList();

            return grnResult;
        }

    }
}



