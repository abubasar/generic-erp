using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Purchase.StockAdjustment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services.Services.Inventory.StockAdjustments
{
    public class StockAdjustmentService : BaseService<StockAdjustment, StockAdjustmentCreationDto, StockAdjustmentUpdateDto, StockAdjustmentRequestModel, StockAdjustmentViewModel>, IStockAdjustmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ICogsCalculationService _cogsCalculationService;
        public StockAdjustmentService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
            INotificationService notificationService, IAccountService accountService,
            IHubContext<BroadcastHub, IHubClient> hubContext, ICogsCalculationService cogsCalculationService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _accountService = accountService;
            _hubContext = hubContext;
            _cogsCalculationService = cogsCalculationService;
        }
        public async Task<StockAdjustmentViewModel> GetByIdAsync(Guid id)
        {
            var stockAdjustment = await _unitOfWork.Repository<StockAdjustment>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Store).Include(x => x.StockAdjustmentDetails).ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (stockAdjustment == null) throw new NotFoundResultException("Stock Adjustment Not Found With this Id");
            return _mapper.Map<StockAdjustmentViewModel>(stockAdjustment);
        }
        public new async Task<Guid> AddAsync(StockAdjustmentCreationDto stockAdjustmentCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            stockAdjustmentCreationDto.AdjustmentDate = stockAdjustmentCreationDto.AdjustmentDate.ToLocal();
            var stockAdjustment = _mapper.Map<StockAdjustment>(stockAdjustmentCreationDto);
            var count = _unitOfWork.Repository<StockAdjustment>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            stockAdjustment.Id = Guid.NewGuid();
            stockAdjustment.Code = "SA" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            stockAdjustment.FinancialYearId = financialYear.Id;
            stockAdjustment.Status = (int)StockAdjustmentStatus.Pending;

            foreach (var item in stockAdjustment.StockAdjustmentDetails)
            {
                item.Id = Guid.NewGuid();
                item.StockAdjustmentId = stockAdjustment.Id;
                await _unitOfWork.Repository<StockAdjustmentDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<StockAdjustment>().AddAsync(stockAdjustment);

            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, stockAdjustment.Id
                , "/purchase/stock-adjustment/" + stockAdjustment.Id, Permissions.StockAdjustments.Check,
                "Stock Adjustment " + stockAdjustment.Code + " is ready for Check", (int)StockAdjustmentStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return stockAdjustment.Id;
        }
        public new async Task<Guid> UpdateAsync(StockAdjustmentUpdateDto stockAdjustmentUpdateDto)
        {
            var dbStockAdjustment = await _unitOfWork.Repository<StockAdjustment>().FindAsync(stockAdjustmentUpdateDto.Id);
            //when date updated,convert it to local date
            if (stockAdjustmentUpdateDto.AdjustmentDate.Equals(dbStockAdjustment.AdjustmentDate) == false)
            {
                stockAdjustmentUpdateDto.AdjustmentDate = stockAdjustmentUpdateDto.AdjustmentDate.ToLocal();
            }
            var stockAdjustment = _mapper.Map(stockAdjustmentUpdateDto, dbStockAdjustment);
            if (dbStockAdjustment.Status == (int)StockAdjustmentStatus.Pending || dbStockAdjustment.Status == (int)StockAdjustmentStatus.Checked)
            {
                await _unitOfWork.Repository<StockAdjustment>().UpdateAsync(stockAdjustment);
                foreach (var item in stockAdjustment.StockAdjustmentDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.StockAdjustmentId = stockAdjustment.Id;
                        await _unitOfWork.Repository<StockAdjustmentDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<StockAdjustmentDetail>().UpdateAsync(item);
                }
                //Delete for StockAdjustmentDetails
                if (!string.IsNullOrEmpty(stockAdjustmentUpdateDto.DeletedStockAdjustmentDetailIds))
                {
                    foreach (var id in stockAdjustmentUpdateDto.DeletedStockAdjustmentDetailIds.Split(',').Where(x => x != ""))
                    {
                        var stockAdjustmentDetail = await _unitOfWork.Repository<StockAdjustmentDetail>().FindAsync(new Guid(id));
                        stockAdjustmentDetail.Deleted = true;
                        await _unitOfWork.Repository<StockAdjustmentDetail>().UpdateAsync(stockAdjustmentDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return stockAdjustment.Id;
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var stockAdjustment = await _unitOfWork.Repository<StockAdjustment>().FindAsync(x => x.Id == id, x => x.Include(x => x.StockAdjustmentDetails));
            if (stockAdjustment == null) throw new NotFoundResultException("Stock Adjustment Not Found With this Id");
            if (stockAdjustment.Status == (int)StockAdjustmentStatus.Pending)
            {
                foreach (var item in stockAdjustment.StockAdjustmentDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<StockAdjustmentDetail>().UpdateAsync(item);
                }
                stockAdjustment.Deleted = true;
                await _unitOfWork.Repository<StockAdjustment>().UpdateAsync(stockAdjustment);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return stockAdjustment.Id;
        }
        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var stockAdjustments = _unitOfWork.Repository<StockAdjustment>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await stockAdjustments.Where(x => x.Status == (int)StockAdjustmentStatus.Pending).CountAsync(),
                CheckedCount = await stockAdjustments.Where(x => x.Status == (int)StockAdjustmentStatus.Checked).CountAsync()
            };
            return response;
        }
        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbStockAdjustment = await _unitOfWork.Repository<StockAdjustment>().FindAsync(id);
            if (dbStockAdjustment.Status == (int)StockAdjustmentStatus.Pending)
            {
                dbStockAdjustment.Status = (int)StockAdjustmentStatus.Checked;
                dbStockAdjustment.CheckedBy = _workContext.GetUserName();
                //update StockAdjustment
                await _unitOfWork.Repository<StockAdjustment>().UpdateAsync(dbStockAdjustment);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbStockAdjustment.Id
                       && x.Status == (int)StockAdjustmentStatus.Pending);

                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbStockAdjustment.Id
                    , "/purchase/stock-adjustment/" + dbStockAdjustment.Id, Permissions.StockAdjustments.Approve,
                    "Stock Adjustment " + dbStockAdjustment.Code + " is ready for Approval", (int)StockAdjustmentStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Stock Adjustment Status has already been checked by " + dbStockAdjustment?.CheckedBy);
        }
        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<StockAdjustment>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Stock Adjustment not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Stock Adjustment does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<StockAdjustment>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)StockAdjustmentStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)StockAdjustmentStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Stock Adjustment already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbStockAdjustment = await _unitOfWork.Repository<StockAdjustment>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.Store).Include(x => x.StockAdjustmentDetails));

            dbStockAdjustment.Status = (int)StockAdjustmentStatus.Approved;
            dbStockAdjustment.ApprovedBy = approvedBy;

            decimal rmCostTotal = 0.0M;
            foreach (var item in dbStockAdjustment.StockAdjustmentDetails)
            {
                decimal rmCost = await StockIncreaseAndDecrease(dbStockAdjustment, item);
                item.AdjustmentValue = rmCost;
                //update Manufacturing Order Detail
                await _unitOfWork.Repository<StockAdjustmentDetail>().UpdateAsync(item);
                rmCostTotal += rmCost;
            }
            dbStockAdjustment.TotalAdjustmentValue = rmCostTotal;
            await InsertTransaction(dbStockAdjustment);
            //update Stock Adjustment
            await _unitOfWork.Repository<StockAdjustment>().UpdateAsync(dbStockAdjustment);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbStockAdjustment.Id
                   && x.Status == (int)StockAdjustmentStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<StockAdjustment>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("StockAdjustment Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Stock Adjustment does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)StockAdjustmentStatus.Checked && dbInfo.Status != (int)StockAdjustmentStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)StockAdjustmentStatus.Checked
                ? (int)StockAdjustmentStatus.Pending
                : (int)StockAdjustmentStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<StockAdjustment>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbStockAdjustment = await _unitOfWork.Repository<StockAdjustment>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.Store).Include(x => x.StockAdjustmentDetails));
            dbStockAdjustment.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)StockAdjustmentStatus.Checked:
                    dbStockAdjustment.CheckedBy = "";
                    break;
                case (int)StockAdjustmentStatus.Approved:
                    dbStockAdjustment.ApprovedBy = "";
                    await DeleteTransaction(dbStockAdjustment.Code);
                    await ReverseStock(dbStockAdjustment);
                    break;
            }

            await _unitOfWork.Repository<StockAdjustment>().UpdateAsync(dbStockAdjustment);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbStockAdjustment.Status;
        }


        private async Task<decimal> StockIncreaseAndDecrease(StockAdjustment stockAdjustment, StockAdjustmentDetail stockAdjustmentDetail)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var product = await _unitOfWork.Repository<Product>().FindAsync(stockAdjustmentDetail.ProductId);
            if (stockAdjustmentDetail.AdjustmentQty == 0) return 0m;
            if (stockAdjustmentDetail.AdjustmentQty > 0)
            {
                //increase
                var queryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => x.StoreId == stockAdjustment.StoreId && x.ProductId == stockAdjustmentDetail.ProductId).OrderByDescending(x => x.StockInDate);
                var averageInRate = 0m;
                if (queryable.Any())
                {
                    var availabeStockQueryable = queryable.Where(x => x.AvailableQty != 0);
                    if (availabeStockQueryable.Any()) averageInRate = availabeStockQueryable.Average(x => x.InRate + x.InTransportCost);
                    else averageInRate = queryable.FirstOrDefault()!.InRate;
                }
                else throw new BadRequestException("No Purchase available for this Item!!!");
                await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                {
                    Id = Guid.NewGuid(),
                    TransactonType = (int)TransactonType.StockAdjustment_Plus,
                    InventoryTypeId = product.InventoryTypeId,
                    ProductTypeId = product.ProductTypeId,
                    ProductId = stockAdjustmentDetail.ProductId,
                    StoreId = stockAdjustment.StoreId,
                    AvailableQty = stockAdjustmentDetail.AdjustmentQty,
                    InQty = stockAdjustmentDetail.AdjustmentQty,
                    InRate = averageInRate,
                    InTransportCost = 0m,
                    BatchNo = stockAdjustment.Code,
                    TransactionId = stockAdjustment.Id,
                    StockInDate = stockAdjustment.AdjustmentDate,
                    TransactionDate = stockAdjustment.AdjustmentDate,
                    Remark = "Stock Adjustment: " + stockAdjustment?.Code + " " + stockAdjustment?.Remark,
                    FinancialYearId = financialYear.Id
                });
                return averageInRate * stockAdjustmentDetail.AdjustmentQty;
            }
            else
            {
                //decrease
                var outQty = Math.Abs(stockAdjustmentDetail.AdjustmentQty);
                var (rmcost, stocks) = _cogsCalculationService.CalculateCOGS(new List<int> { (int)TransactonType.Purchase, (int)TransactonType.StockAdjustment_Plus, (int)TransactonType.Production, (int)TransactonType.TransferReceive, (int)TransactonType.SaleReturn }, product, stockAdjustment.StoreId, outQty);
                foreach (var stock in stocks)
                {
                    await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
                }
                await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                {
                    Id = Guid.NewGuid(),
                    TransactonType = (int)TransactonType.StockAdjustment_Minus,
                    InventoryTypeId = product.InventoryTypeId,
                    ProductTypeId = product.ProductTypeId,
                    ProductId = stockAdjustmentDetail.ProductId,
                    StoreId = stockAdjustment.StoreId,
                    OutQty = outQty,
                    OutRate = rmcost / outQty,
                    BatchNo = string.Join(",", stocks.Select(x => x.BatchNo)),
                    TransactionId = stockAdjustment.Id,
                    StockInDate = stocks.First().StockInDate,
                    TransactionDate = stockAdjustment.AdjustmentDate,
                    Remark = "Stock Adjustment: " + stockAdjustment?.Code + " " + stockAdjustment?.Remark,
                    FinancialYearId = financialYear.Id
                });
                return -rmcost;
            }

        }
        private async Task ReverseStock(StockAdjustment stockAdjustment)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var stockAdjustmentMinusStocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Minus && x.TransactionId == stockAdjustment.Id).ToListAsync();
            if (stockAdjustmentMinusStocks.Any())
            {
                foreach (var stock in stockAdjustmentMinusStocks)
                {
                    await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                    {
                        Id = Guid.NewGuid(),
                        TransactonType = (int)TransactonType.StockAdjustment_Plus,
                        InventoryTypeId = stock.InventoryTypeId,
                        ProductTypeId = stock.ProductTypeId,
                        ProductId = stock.ProductId,
                        StoreId = stock.StoreId,
                        AvailableQty = stock.OutQty,
                        InQty = stock.OutQty,
                        InRate = stock.OutRate,
                        BatchNo = stock.BatchNo,
                        TransactionId = stockAdjustment.Id,
                        StockInDate = stock.StockInDate,
                        TransactionDate = DateTime.Now,
                        Remark = "Reverse Stock for Stock Adjustment Minus",
                        IsUnpostedEntry = true,
                        FinancialYearId = financialYear.Id
                    });
                }
                await _unitOfWork.Repository<Stock>().DeleteAsync(stockAdjustmentMinusStocks);
            }
            var stockAdjustmentPlusStocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                     .Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Plus && x.TransactionId == stockAdjustment.Id).ToListAsync();
            if (stockAdjustmentPlusStocks.Any())
            {
                foreach (var stock in stockAdjustmentPlusStocks)
                {
                    if (stock.AvailableQty == stock.InQty)
                    {
                        await _unitOfWork.Repository<Stock>().DeleteAsync(stock.Id);
                    }
                    else throw new BadRequestException("Stock Already Occupied ! Not Possible to Unpost");
                }
            }

        }

        private async Task InsertTransaction(StockAdjustment stockAdjustment)
        {
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var inventoryAdjustAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.InventoryAdjust.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            if (stockAdjustment.Store.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid())
            {
                var rawMaterialsInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.RawMaterialsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                if (stockAdjustment.TotalAdjustmentValue > 0m)
                {
                    contraAccountNamesForCredit.Append(rawMaterialsInventoryAccountName);
                    contraAccountIdsForCredit.Append(AccountHeadConstants.RawMaterialsInventory);
                    contraAccountNamesForDebit.Append(inventoryAdjustAccountName);
                    contraAccountIdsForDebit.Append(AccountHeadConstants.InventoryAdjust);

                    //RM Inventory account debit
                    await _accountService.HitAccount(_unitOfWork, null, stockAdjustment.Code, stockAdjustment.Remark,
                        Guid.Parse(AccountHeadConstants.RawMaterialsInventory), stockAdjustment.TotalAdjustmentValue, 0, stockAdjustment.AdjustmentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                    //inventory adjustment account credit
                    await _accountService.HitAccount(_unitOfWork, null, stockAdjustment.Code, stockAdjustment.Remark,
                     Guid.Parse(AccountHeadConstants.InventoryAdjust), 0, stockAdjustment.TotalAdjustmentValue, stockAdjustment.AdjustmentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }
                else
                {
                    contraAccountNamesForCredit.Append(inventoryAdjustAccountName);
                    contraAccountIdsForCredit.Append(AccountHeadConstants.InventoryAdjust);
                    contraAccountNamesForDebit.Append(rawMaterialsInventoryAccountName);
                    contraAccountIdsForDebit.Append(AccountHeadConstants.RawMaterialsInventory);
                    //RM Inventory account credit
                    await _accountService.HitAccount(_unitOfWork, null, stockAdjustment.Code, stockAdjustment.Remark,
                        Guid.Parse(AccountHeadConstants.RawMaterialsInventory), 0, Math.Abs(stockAdjustment.TotalAdjustmentValue), stockAdjustment.AdjustmentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                    //inventory adjustment account debit
                    await _accountService.HitAccount(_unitOfWork, null, stockAdjustment.Code, stockAdjustment.Remark,
                     Guid.Parse(AccountHeadConstants.InventoryAdjust), Math.Abs(stockAdjustment.TotalAdjustmentValue), 0, stockAdjustment.AdjustmentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                }
            }
            else if (stockAdjustment.Store.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid())
            {
                var finishedGoodsInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.FinishedGoodsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                if (stockAdjustment.TotalAdjustmentValue > 0m)
                {
                    contraAccountNamesForCredit.Append(finishedGoodsInventoryAccountName);
                    contraAccountIdsForCredit.Append(AccountHeadConstants.FinishedGoodsInventory);
                    contraAccountNamesForDebit.Append(inventoryAdjustAccountName);
                    contraAccountIdsForDebit.Append(AccountHeadConstants.InventoryAdjust);
                    //FG Inventory account debit
                    await _accountService.HitAccount(_unitOfWork, null, stockAdjustment.Code, stockAdjustment.Remark,
                        Guid.Parse(AccountHeadConstants.FinishedGoodsInventory), stockAdjustment.TotalAdjustmentValue, 0, stockAdjustment.AdjustmentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                    //inventory adjustment account credit
                    await _accountService.HitAccount(_unitOfWork, null, stockAdjustment.Code, stockAdjustment.Remark,
                     Guid.Parse(AccountHeadConstants.InventoryAdjust), 0, stockAdjustment.TotalAdjustmentValue, stockAdjustment.AdjustmentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }
                else
                {
                    contraAccountNamesForCredit.Append(inventoryAdjustAccountName);
                    contraAccountIdsForCredit.Append(AccountHeadConstants.InventoryAdjust);
                    contraAccountNamesForDebit.Append(finishedGoodsInventoryAccountName);
                    contraAccountIdsForDebit.Append(AccountHeadConstants.FinishedGoodsInventory);
                    //FG Inventory account credit
                    await _accountService.HitAccount(_unitOfWork, null, stockAdjustment.Code, stockAdjustment.Remark,
                        Guid.Parse(AccountHeadConstants.FinishedGoodsInventory), 0, Math.Abs(stockAdjustment.TotalAdjustmentValue), stockAdjustment.AdjustmentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                    //inventory adjustment account debit
                    await _accountService.HitAccount(_unitOfWork, null, stockAdjustment.Code, stockAdjustment.Remark,
                     Guid.Parse(AccountHeadConstants.InventoryAdjust), Math.Abs(stockAdjustment.TotalAdjustmentValue), 0, stockAdjustment.AdjustmentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                }
            }
            else throw new BadRequestException("This Feature is only for Raw Materials and Finished Goods!!");
        }
        private async Task DeleteTransaction(string code)
        {
            var stockAdjustment = await _unitOfWork.Repository<StockAdjustment>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (stockAdjustment is null) throw new NotFoundResultException("Stock Adjustment Not Found With this Stock Adjustment Code");
            var transactions = await _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == stockAdjustment.Code).ToListAsync();
            await _unitOfWork.Repository<Transaction>().DeleteAsync(transactions);
        }
    }
}
