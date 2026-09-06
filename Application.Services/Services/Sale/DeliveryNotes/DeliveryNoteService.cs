using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Sale.DeliveryNote;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.DeliveryNote;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Sale.DeliveryNotes
{
    public class DeliveryNoteService : BaseService<DeliveryNote, DeliveryNoteCreationDto, DeliveryNoteUpdateDto, DeliveryNoteRequestModel, DeliveryNoteViewModel>, IDeliveryNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ICogsCalculationService _cogsCalculationService;

        public DeliveryNoteService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
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

        public async Task<DeliveryNoteAggregatorModel> PrepareDeliveryNoteAggregatorModel(DeliveryNoteRequestModel deliveryNoterRequest)
        {
            var deliveryNoteQueryable = _unitOfWork.Repository<DeliveryNote>().TableNoTracking().Where(deliveryNoterRequest.GetExpression());
            return new DeliveryNoteAggregatorModel
            {
                AggregatorSubtotal = await deliveryNoteQueryable.SumAsync(x => x.Subtotal),
                AggregatorDiscount = await deliveryNoteQueryable.SumAsync(x => x.Discount),
                AggregatorOfferDiscount = await deliveryNoteQueryable.SumAsync(x => x.OfferDiscount),
                AggregatorOtherDiscount = await deliveryNoteQueryable.SumAsync(x => x.OtherDiscount),
                AggregatorTotal = await deliveryNoteQueryable.SumAsync(x => x.Total),
                AggregatorTransportationCost = await deliveryNoteQueryable.SumAsync(x => x.TransportationCost),
                AggregatorDepoCharge = await deliveryNoteQueryable.SumAsync(x => x.DepoCharge),
                AggregatorNetTotal = await deliveryNoteQueryable.SumAsync(x => x.NetTotal),
            };
        }

        public async Task<DeliveryNoteViewModel> GetByIdAsync(Guid id)
        {
            var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Customer).Include(x => x.Store)
                .Include(blog => blog.DeliveryNoteDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit));
            if (deliveryNote == null) throw new NotFoundResultException("Delivery Note Not Found With this Id");
            return _mapper.Map<DeliveryNoteViewModel>(deliveryNote);
        }

        public new async Task<Guid> AddAsync(DeliveryNoteCreationDto deliveryNoteCreationDto)
        {
            if (!string.IsNullOrWhiteSpace(deliveryNoteCreationDto.SaleOrderNo))
            {
                var dbDeliveryNote = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking().FirstOrDefaultAsync(x => x.SaleOrderNo == deliveryNoteCreationDto.SaleOrderNo
                && new int[] { (int)DeliveryNoteStatus.Pending, (int)DeliveryNoteStatus.Checked }.Contains(x.Status));
                if (dbDeliveryNote is not null) throw new BadRequestException("You have already created one Delivery Note#" + dbDeliveryNote.DeliveryNoteNo + " for Sale Order#" + deliveryNoteCreationDto.SaleOrderNo + ".Please Approve this Delivery Note first");

            }
            var financialYear = _workContext.GetCurrentFinancialYear();
            deliveryNoteCreationDto.DeliveryDate = deliveryNoteCreationDto.DeliveryDate.ToLocal();
            var deliveryNote = _mapper.Map<DeliveryNote>(deliveryNoteCreationDto);
            var count = _unitOfWork.Repository<DeliveryNote>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            deliveryNote.Id = Guid.NewGuid();
            deliveryNote.DeliveryNoteNo = "DN" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            deliveryNote.FinancialYearId = financialYear.Id;
            //deliveryNote.Status = (int)DeliveryNoteStatus.Approved;
            //deliveryNote.ApprovedBy = _workContext.GetUserName();
            deliveryNote.Status = (int)DeliveryNoteStatus.Pending;
            var customer = await _unitOfWork.Repository<Account>().FindAsync(deliveryNoteCreationDto.CustomerId);
            deliveryNote.CustomerMarketingOfficerId = customer?.CustomerMarketingOfficerId;
            foreach (var item in deliveryNote.DeliveryNoteDetails)
            {
                if (item.DeliveryPrimaryQuantity == 0 || item.DeliveryQuantity == 0) continue;
                item.Id = Guid.NewGuid();
                item.DeliveryNoteId = deliveryNote.Id;
                //monthly,yearly,target discount entry start
                var customerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                                                      .TableNoTracking().SingleOrDefaultAsync(x => x.CustomerId == deliveryNote.CustomerId && x.IsActive);
                if (customerWiseProductDiscount is not null)
                {
                    var discountDetail = await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>()
                                                      .TableNoTracking()
                                                      .SingleOrDefaultAsync(x => x.CustomerWiseProductDiscountId == customerWiseProductDiscount.Id
                                                      && x.ProductId == item.ProductId);
                    if (discountDetail is not null)
                    {
                        item.MonthlyDiscountPerUnit = discountDetail.MonthlyDiscount;
                        item.YearlyDiscountPerUnit = discountDetail.YearlyDiscount;
                        item.TargetDiscountPerUnit = discountDetail.TargetDiscount;
                    }
                    else
                    {
                        item.MonthlyDiscountPerUnit = 0;
                        item.YearlyDiscountPerUnit = 0;
                        item.TargetDiscountPerUnit = 0;
                    }
                }
                else
                {
                    item.MonthlyDiscountPerUnit = 0;
                    item.YearlyDiscountPerUnit = 0;
                    item.TargetDiscountPerUnit = 0;
                }
                //monthly,yearly,target discount entry end
                await _unitOfWork.Repository<DeliveryNoteDetail>().AddAsync(item);
            }
            deliveryNote.DeliveryNoteDetails = null;
            await _unitOfWork.Repository<DeliveryNote>().AddAsync(deliveryNote);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, deliveryNote.Id
                , "/sales/delivery-note/" + deliveryNote.Id, SecondaryPermissions.DeliveryNotes.Check,
                "Delivery Note " + deliveryNote.DeliveryNoteNo + " is ready for Check", (int)DeliveryNoteStatus.Pending);

            //auto approve
            //await AutoApproveAsync(deliveryNote);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return deliveryNote.Id;
        }

        public new async Task<Guid> UpdateAsync(DeliveryNoteUpdateDto deliveryNoteUpdateDto)
        {
            var dbDeliveryNote = await _unitOfWork.Repository<DeliveryNote>().FindAsync(deliveryNoteUpdateDto.Id);
            //when date updated,convert it to local date
            if (deliveryNoteUpdateDto.DeliveryDate.Equals(dbDeliveryNote.DeliveryDate) == false)
            {
                deliveryNoteUpdateDto.DeliveryDate = deliveryNoteUpdateDto.DeliveryDate.ToLocal();
            }
            var deliveryNote = _mapper.Map(deliveryNoteUpdateDto, dbDeliveryNote);
            if (dbDeliveryNote.Status == (int)DeliveryNoteStatus.Pending || dbDeliveryNote.Status == (int)DeliveryNoteStatus.Checked)
            {
                await _unitOfWork.Repository<DeliveryNote>().UpdateAsync(deliveryNote);

                if (deliveryNote.DeliveryNoteDetails.Count == 1 && deliveryNote.DeliveryNoteDetails.First().DeliveryPrimaryQuantity == 0)
                    throw new BadRequestException("Delivery Bag Quantity can not be 0");
                foreach (var item in deliveryNote.DeliveryNoteDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.DeliveryNoteId = deliveryNote.Id;
                        await _unitOfWork.Repository<DeliveryNoteDetail>().AddAsync(item);
                    }
                    else
                    {
                        if (item.DeliveryPrimaryQuantity == 0 || item.DeliveryQuantity == 0)
                        {
                            item.Deleted = true;
                        }
                        await _unitOfWork.Repository<DeliveryNoteDetail>().UpdateAsync(item);
                    }

                }
                //Delete for DeliveryNoteDetails
                if (!string.IsNullOrEmpty(deliveryNoteUpdateDto.DeletedDeliveryNoteDetailIds))
                {
                    foreach (var id in deliveryNoteUpdateDto.DeletedDeliveryNoteDetailIds.Split(',').Where(x => x != ""))
                    {
                        var deliveryNoteDetail = await _unitOfWork.Repository<DeliveryNoteDetail>().FindAsync(new Guid(id));
                        deliveryNoteDetail.Deleted = true;
                        await _unitOfWork.Repository<DeliveryNoteDetail>().UpdateAsync(deliveryNoteDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return deliveryNote.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().FindAsync(x => x.Id == id, x => x.Include(x => x.DeliveryNoteDetails));
            if (deliveryNote == null) throw new NotFoundResultException("DeliveryNote Not Found With this Id");
            if (deliveryNote.Status == (int)DeliveryNoteStatus.Pending)
            {
                foreach (var item in deliveryNote.DeliveryNoteDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<DeliveryNoteDetail>().UpdateAsync(item);
                }
                deliveryNote.Deleted = true;
                await _unitOfWork.Repository<DeliveryNote>().UpdateAsync(deliveryNote);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return deliveryNote.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var deliveryNotes = _unitOfWork.Repository<DeliveryNote>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await deliveryNotes.Where(x => x.Status == (int)DeliveryNoteStatus.Pending).CountAsync(),
                CheckedCount = await deliveryNotes.Where(x => x.Status == (int)DeliveryNoteStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbDeliveryNote = await _unitOfWork.Repository<DeliveryNote>().FindAsync(id);
            if (dbDeliveryNote.Status == (int)DeliveryNoteStatus.Pending)
            {
                dbDeliveryNote.Status = (int)DeliveryNoteStatus.Checked;
                dbDeliveryNote.CheckedBy = _workContext.GetUserName();
                //update DeliveryNote
                await _unitOfWork.Repository<DeliveryNote>().UpdateAsync(dbDeliveryNote);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbDeliveryNote.Id
                       && x.Status == (int)DeliveryNoteStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbDeliveryNote.Id
                    , "/sales/delivery-note/" + dbDeliveryNote.Id, SecondaryPermissions.DeliveryNotes.Approve,
                    "Delivery Note " + dbDeliveryNote.DeliveryNoteNo + " is ready for Approval", (int)DeliveryNoteStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Delivery Note Status has already been checked by " + dbDeliveryNote?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // 🔒 Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // 🔹 Load minimal data first (no tracking)
            var dbDeliveryNote = await _unitOfWork.Repository<DeliveryNote>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.FinancialYearId,
                    x.SaleOrderNo,
                    x.Status,
                    x.ApprovedBy
                })
                .SingleOrDefaultAsync();

            if (dbDeliveryNote == null)
                throw new NotFoundResultException("Delivery Note not found.");

            if (financialYear.Id != dbDeliveryNote.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Delivery Note does not belong to the current financial year.");

            // 🔹 Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<DeliveryNote>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)DeliveryNoteStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)DeliveryNoteStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Delivery Note already approved by {dbDeliveryNote.ApprovedBy}");

            // 🔥 From here → ONLY ONE THREAD EXECUTES

            // 🔹 Reload FULL entity WITH tracking (inside transaction)
            var deliveryNote = await _unitOfWork.Repository<DeliveryNote>()
                .FindAsync(x => x.Id == id,
                    x => x.Include(d => d.DeliveryNoteDetails));

            var saleOrder = await _unitOfWork.Repository<SaleOrder>()
                .TableNoTracking()
                .Include(x => x.SaleOrderDetails)
                .SingleOrDefaultAsync(x => x.SaleOrderNo == deliveryNote.SaleOrderNo);

            if (saleOrder == null)
                throw new NotFoundResultException("Sale Order not found associated with this Delivery Note " + deliveryNote.SaleOrderNo);

            deliveryNote.Status = (int)DeliveryNoteStatus.Approved;
            deliveryNote.ApprovedBy = approvedBy;

            #region 🔄 Business Logic (NOW SAFE)

            int totalDeliveredBagQuantity =
                saleOrder.SaleOrderDetails.Sum(x => x.DeliveredPrimaryQuantity + x.DeliveredPrimaryBonusQuantity);

            decimal cogs_Total = 0.0M;

            foreach (var item in deliveryNote.DeliveryNoteDetails)
            {
                // 🔒 These are now safe inside transaction
                await IncreaseSaleOrderDetailDeliveredQuantityByDeliveryQuantity(item);

                totalDeliveredBagQuantity +=
                    (item.DeliveryPrimaryQuantity + item.DeliveryPrimaryBonusQuantity);

                decimal cogs = await InsertStock(deliveryNote, item);
                cogs_Total += cogs;
            }

            await ChangeSaleOrderStatus(deliveryNote, saleOrder, totalDeliveredBagQuantity);

            await InsertTransaction(deliveryNote, cogs_Total);

            #endregion

            // 🔹 Remove notification
            await _unitOfWork.Repository<Notification>()
                .DeleteAsync(x =>
                    x.GuidId == deliveryNote.Id &&
                    x.Status == (int)DeliveryNoteStatus.Checked);

            // 🔹 Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // 🔒 Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // 🔹 Notify clients (outside critical DB section ideally)
            await _hubContext.Clients.All.BroadcastMessage();

            return isApproved;
        }
        private async Task<bool> IsDuplicateRequest(Guid deliveryNoteId)
        {
            var stockInserted = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .AnyAsync(x => x.TransactonType == (int)TransactonType.Sale
                       && x.TransactionId == deliveryNoteId);
            return stockInserted;
        }

        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Load minimal data (no tracking)
            var dbInfo = await _unitOfWork.Repository<DeliveryNote>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.SaleOrderNo, x.DeliveryNoteNo })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Delivery Note not found.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Delivery Note does not belong to the current financial year.");

            var saleOrder = await _unitOfWork.Repository<SaleOrder>().TableNoTracking().Include(x => x.SaleOrderDetails).SingleOrDefaultAsync(x => x.SaleOrderNo == dbInfo.SaleOrderNo);
            if (saleOrder is null) throw new NotFoundResultException("Sale Order not found associated with this Delivery Note " + dbInfo.SaleOrderNo);

            if (dbInfo.Status != fromStatus) throw new BadRequestException("Delivery Note status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)DeliveryNoteStatus.Checked && dbInfo.Status != (int)DeliveryNoteStatus.Approved)
                throw new BadRequestException("Only Checked or Approved Delivery Note can be unposted.");

            var newStatus = dbInfo.Status == (int)DeliveryNoteStatus.Approved
                ? (int)DeliveryNoteStatus.Checked
                : (int)DeliveryNoteStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<DeliveryNote>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Delivery Note status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbDeliveryNote = await _unitOfWork.Repository<DeliveryNote>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.DeliveryNoteDetails));

            dbDeliveryNote.Status = newStatus;
            dbDeliveryNote.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)DeliveryNoteStatus.Checked:
                    dbDeliveryNote.CheckedBy = "";
                    break;
                case (int)DeliveryNoteStatus.Approved:
                    dbDeliveryNote.ApprovedBy = "";
                    #region Delete Related Entity
                    await DeleteTransaction(dbDeliveryNote.DeliveryNoteNo);
                    await ReverseStock(dbDeliveryNote);
                    int totalDeliveredBagQuantity = saleOrder.SaleOrderDetails.Sum(x => x.DeliveredPrimaryQuantity + x.DeliveredPrimaryBonusQuantity);
                    foreach (var item in dbDeliveryNote.DeliveryNoteDetails)
                    {
                        await ReduceSaleOrderDetailDeliveredQuantityByDeliveryQuantity(item);
                        totalDeliveredBagQuantity -= (item.DeliveryPrimaryQuantity + item.DeliveryPrimaryBonusQuantity);
                    }
                    await ChangeSaleOrderStatus(dbDeliveryNote, saleOrder, totalDeliveredBagQuantity);
                    #endregion
                    break;
            }

            await _unitOfWork.Repository<DeliveryNote>().UpdateAsync(dbDeliveryNote);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbDeliveryNote.Status;
        }
        private async Task ChangeSaleOrderStatus(DeliveryNote deliveryNote, SaleOrder saleOrder, int totalDeliveredBagQuantity)
        {
            if (!string.IsNullOrWhiteSpace(deliveryNote.SaleOrderNo))
            {
                var sumOrderedBagQuantity = saleOrder!.SaleOrderDetails.Sum(x => x.PrimaryQuantity + x.PrimaryBonusQuantity);
                if (totalDeliveredBagQuantity > sumOrderedBagQuantity) throw new BadRequestException("Delivery Qty exceeded the Ordered Qty!!");
                if (sumOrderedBagQuantity == totalDeliveredBagQuantity)
                    saleOrder!.Status = (int)SaleOrderStatus.Item_Completely_Delivered;
                else if (totalDeliveredBagQuantity == 0)
                    saleOrder!.Status = (int)SaleOrderStatus.Approved;
                else saleOrder!.Status = (int)SaleOrderStatus.Item_Partially_Delivered;
                saleOrder.SaleOrderDetails = null;
                await _unitOfWork.Repository<SaleOrder>().UpdateAsync(saleOrder);
            }
        }

        private async Task<decimal> InsertStock(DeliveryNote deliveryNote, DeliveryNoteDetail item)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var deliveryQty = item.DeliveryQuantity + (item.DeliveryPrimaryBonusQuantity * item.BagWeight);
            var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
            var (cogs, stocks) = _cogsCalculationService.CalculateCOGS(new List<int> { (int)TransactonType.Purchase, (int)TransactonType.Production, (int)TransactonType.TransferReceive, (int)TransactonType.SaleReturn, (int)TransactonType.StockAdjustment_Plus }, product, deliveryNote.StoreId, deliveryQty);
            foreach (var stock in stocks)
            {
                await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
            }
            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.Sale,
                InventoryTypeId = product.InventoryTypeId,
                ProductTypeId = product.ProductTypeId,
                ProductId = item.ProductId,
                StoreId = deliveryNote.StoreId,
                OutQty = deliveryQty,
                OutRate = cogs / deliveryQty,
                BatchNo = string.Join(",", stocks.Select(x => x.BatchNo)),
                TransactionId = deliveryNote.Id,
                StockInDate = stocks.First().StockInDate,
                TransactionDate = deliveryNote.DeliveryDate,
                Remark = "Delivery Note#" + deliveryNote.DeliveryNoteNo,
                FinancialYearId = financialYear.Id
            });
            return cogs;
        }

        private async Task ReverseStock(DeliveryNote deliveryNote)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.Sale && x.TransactionId == deliveryNote.Id).ToListAsync();
            if (stocks.Any())
            {
                foreach (var stock in stocks)
                {
                    var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == stock.ProductId);
                    await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                    {
                        Id = Guid.NewGuid(),
                        TransactonType = (product.IsPurchaseProduct && product.IsSaleProduct) || product.IsSaleProduct
                        ? (int)TransactonType.Purchase
                        : (int)TransactonType.Production,
                        InventoryTypeId = stock.InventoryTypeId,
                        ProductTypeId = stock.ProductTypeId,
                        ProductId = stock.ProductId,
                        StoreId = stock.StoreId,
                        AvailableQty = stock.OutQty,
                        InQty = stock.OutQty,
                        InRate = stock.OutRate,
                        BatchNo = stock.BatchNo,
                        TransactionId = deliveryNote.Id,
                        StockInDate = stock.StockInDate,
                        TransactionDate = DateTime.Now,
                        Remark = "Reverse Stock for Delivery Note#" + deliveryNote.DeliveryNoteNo + " Unposted",
                        IsUnpostedEntry = true,
                        FinancialYearId = financialYear.Id
                    });
                }
                await _unitOfWork.Repository<Stock>().DeleteAsync(stocks);
            }
        }

        private async Task InsertTransaction(DeliveryNote deliveryNote, decimal cogs_Total)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in deliveryNote!.DeliveryNoteDetails)
            {
                var product = await _unitOfWork.Repository<Product>().FindAsync(x => x.Id == item.ProductId);
                var measurementUnit = await _unitOfWork.Repository<MeasurementUnit>().FindAsync(x => x.Id == product.MeasurementUnitId);
                //sb.Append("Delivery Note- ");
                sb.Append(product?.Name);
                sb.Append(", ");
                sb.Append(item?.DeliveryQuantity);
                sb.Append(" ");
                sb.Append(measurementUnit?.Name);
                sb.Append(" @");
                sb.Append(item?.NetRate);
            }

            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var cogsAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.COGS_Standard.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            var finishedGoodsInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.FinishedGoodsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            //prepare contra account info for credit
            contraAccountNamesForCredit.Append(cogsAccountName);
            contraAccountIdsForCredit.Append(AccountHeadConstants.COGS_Standard);
            //prepare contra account info for debit
            contraAccountNamesForDebit.Append(finishedGoodsInventoryAccountName);
            contraAccountIdsForDebit.Append(AccountHeadConstants.FinishedGoodsInventory);

            //cogs account debit
            await _accountService.HitAccount(_unitOfWork, null, deliveryNote.DeliveryNoteNo, sb.ToString(),
                 Guid.Parse(AccountHeadConstants.COGS_Standard), cogs_Total, 0, deliveryNote.DeliveryDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

            //Finished Good Inventory account credit
            await _accountService.HitAccount(_unitOfWork, null, deliveryNote.DeliveryNoteNo, sb.ToString(),
                Guid.Parse(AccountHeadConstants.FinishedGoodsInventory), 0, cogs_Total, deliveryNote.DeliveryDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
        }

        private async Task DeleteTransaction(string deliveryNoteNo)
        {
            var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);
            if (deliveryNote is null) throw new NotFoundResultException("Delivery Note Not Found With this deliveryNote No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == deliveryNote.DeliveryNoteNo);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }

        }

        private async Task ReduceSaleOrderDetailDeliveredQuantityByDeliveryQuantity(DeliveryNoteDetail deliveryNoteDetail)
        {
            if (deliveryNoteDetail.SaleOrderDetailId.HasValue)
            {
                var saleOrderDetail = await _unitOfWork.Repository<SaleOrderDetail>().FindAsync(x => x.Id == deliveryNoteDetail.SaleOrderDetailId.Value);
                saleOrderDetail.DeliveredPrimaryQuantity -= deliveryNoteDetail.DeliveryPrimaryQuantity;
                saleOrderDetail.DeliveredPrimaryBonusQuantity -= deliveryNoteDetail.DeliveryPrimaryBonusQuantity;
                await _unitOfWork.Repository<SaleOrderDetail>().UpdateAsync(saleOrderDetail);
            }
        }

        private async Task IncreaseSaleOrderDetailDeliveredQuantityByDeliveryQuantity(DeliveryNoteDetail deliveryNoteDetail)
        {
            if (deliveryNoteDetail.SaleOrderDetailId.HasValue)
            {
                var saleOrderDetail = await _unitOfWork.Repository<SaleOrderDetail>().FindAsync(x => x.Id == deliveryNoteDetail.SaleOrderDetailId.Value);
                var orderedBagQuantity = saleOrderDetail.PrimaryQuantity + saleOrderDetail.PrimaryBonusQuantity;
                saleOrderDetail.DeliveredPrimaryQuantity += deliveryNoteDetail.DeliveryPrimaryQuantity;
                saleOrderDetail.DeliveredPrimaryBonusQuantity += deliveryNoteDetail.DeliveryPrimaryBonusQuantity;
                var totalDeliveredBagQuantity = saleOrderDetail.DeliveredPrimaryQuantity + saleOrderDetail.DeliveredPrimaryBonusQuantity;
                if (totalDeliveredBagQuantity > orderedBagQuantity) throw new BadRequestException("The delivered quantity for " + saleOrderDetail.Product.Name + " exceeds the ordered quantity.!!");
                await _unitOfWork.Repository<SaleOrderDetail>().UpdateAsync(saleOrderDetail);
            }
            else throw new NotFoundResultException("Associated Sale Order Not Found!!");
        }


    }
}
