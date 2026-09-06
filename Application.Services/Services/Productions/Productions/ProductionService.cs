using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Production.Production;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Production.Production;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Text;

namespace Application.Services.Services.Productions.Productions
{
    public class ProductionService : BaseService<Production, ProductionCreationDto, ProductionUpdateDto, ProductionRequestModel, ProductionViewModel>, IProductionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ICogsCalculationService _cogsCalculationService;

        public ProductionService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
            INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext, IAccountService accountService, ICogsCalculationService cogsCalculationService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
            _cogsCalculationService = cogsCalculationService;
        }

        public async Task<ProductionAggregatorModel> PrepareProductionAggregatorModel(ProductionRequestModel productionRequest)
        {
            var productionQueryable = _unitOfWork.Repository<Production>().TableNoTracking().Where(productionRequest.GetExpression());
            return new ProductionAggregatorModel
            {
                AggregatorExtraDamageQuantity = await productionQueryable.SumAsync(x => x.ExtraDamageQuantity),
                AggregatorTotalRmused = await productionQueryable.SumAsync(x => x.TotalRmused),
                AggregatorRmCost = await productionQueryable.SumAsync(x => x.RmCost),
                AggregatorProductionQuantity = await productionQueryable.SumAsync(x => x.ProductionQuantity),
                AggregatorActualProductionQuantity = await productionQueryable.SumAsync(x => x.ActualProductionQuantity),
                AggregatorTotalCost = await productionQueryable.SumAsync(x => x.TotalCost),
                AggregatorTotalAdjustmentQuantity = await productionQueryable.SumAsync(x => x.TotalAdjustmentQuantity),
                AggregatorTotalAdjustmentCost = await productionQueryable.SumAsync(x => x.TotalAdjustmentCost),
            };
        }

        public async Task<ProductionViewModel> GetByIdAsync(Guid id)
        {
            var production = await _unitOfWork.Repository<Production>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.FinishedProduct).Include(x => x.Fgstore).Include(x => x.ProductionDetails.OrderBy(x => x.RawMaterial.Name))
                .ThenInclude(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit));
            if (production == null) throw new NotFoundResultException("Production Not Found With this Id");
            return _mapper.Map<ProductionViewModel>(production);
        }

        public new async Task<Guid> AddAsync(ProductionCreationDto productionCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var productionExists = await _unitOfWork.Repository<Production>().TableNoTracking().AnyAsync(x => x.ManufacturingOrderNo == productionCreationDto.ManufacturingOrderNo);
            if (productionExists) throw new BadRequestException("You have already created production using this Manufacturing Order# " + productionCreationDto.ManufacturingOrderNo);
            productionCreationDto.ProductionDate = productionCreationDto.ProductionDate.ToLocal();
            productionCreationDto.StartDateTime = productionCreationDto.StartDateTime?.ToLocal();
            productionCreationDto.EndDateTime = productionCreationDto.EndDateTime?.ToLocal();
            if ((productionCreationDto.EndDateTime!.Value.TrimSecondsAndMilliseconds() - productionCreationDto.StartDateTime!.Value.TrimSecondsAndMilliseconds()).TotalMinutes == 0) throw new BadRequestException("StartDateTime & EndDateTime can not be Equal.");
            var production = _mapper.Map<Production>(productionCreationDto);
            var count = _unitOfWork.Repository<Production>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            production.Id = Guid.NewGuid();
            production.ProductionNo = "P" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            production.FinancialYearId = financialYear.Id;
            production.Status = (int)ProductionStatus.Pending;
            foreach (var item in production.ProductionDetails)
            {
                item.Id = Guid.NewGuid();
                item.ProductionId = production.Id;
                await _unitOfWork.Repository<ProductionDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<Production>().AddAsync(production);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, production.Id
                , "/production/production/" + production.Id, Permissions.Productions.Check,
                "Production " + production.ProductionNo + " is ready for Check", (int)ProductionStatus.Pending);

            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return production.Id;
        }

        public new async Task<Guid> UpdateAsync(ProductionUpdateDto productionUpdateDto)
        {
            var dbProduction = await _unitOfWork.Repository<Production>().FindAsync(productionUpdateDto.Id);
            //when date updated,convert it to local date
            if (productionUpdateDto.ProductionDate.Equals(dbProduction.ProductionDate) == false)
            {
                productionUpdateDto.ProductionDate = productionUpdateDto.ProductionDate.ToLocal();
            }
            //When StartDateTime updated, Convert it to local date
            if (productionUpdateDto.StartDateTime.Equals(dbProduction.StartDateTime) == false)
            {
                productionUpdateDto.StartDateTime = productionUpdateDto.StartDateTime?.ToLocal();
            }
            //When EndDateTime updated, Convert it to local date
            if (productionUpdateDto.EndDateTime.Equals(dbProduction.EndDateTime) == false)
            {
                productionUpdateDto.EndDateTime = productionUpdateDto.EndDateTime?.ToLocal();
            }
            if ((productionUpdateDto.EndDateTime!.Value.TrimSecondsAndMilliseconds() - productionUpdateDto.StartDateTime!.Value.TrimSecondsAndMilliseconds()).TotalMinutes == 0) throw new BadRequestException("StartDateTime & EndDateTime can not be Equal.");

            if (dbProduction == null) throw new NotFoundResultException("Production Not Found With this Id");
            var production = _mapper.Map(productionUpdateDto, dbProduction);
            if (dbProduction.Status == (int)ProductionStatus.Pending || dbProduction.Status == (int)ProductionStatus.Checked)
            {
                await _unitOfWork.Repository<Production>().UpdateAsync(production);
                foreach (var item in production.ProductionDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.ProductionId = production.Id;
                        await _unitOfWork.Repository<ProductionDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<ProductionDetail>().UpdateAsync(item);
                }
                //Delete for ProductionDetails
                if (!string.IsNullOrEmpty(productionUpdateDto.DeletedProductionDetailIds))
                {
                    foreach (var id in productionUpdateDto.DeletedProductionDetailIds.Split(',').Where(x => x != ""))
                    {
                        var productionDetail = await _unitOfWork.Repository<ProductionDetail>().FindAsync(new Guid(id));
                        productionDetail.Deleted = true;
                        await _unitOfWork.Repository<ProductionDetail>().UpdateAsync(productionDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return production.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var production = await _unitOfWork.Repository<Production>().FindAsync(x => x.Id == id, x => x.Include(x => x.ProductionDetails));
            if (production == null) throw new NotFoundResultException("Production Not Found With this Id");
            if (production.Status == (int)ProductionStatus.Pending)
            {
                foreach (var item in production.ProductionDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<ProductionDetail>().UpdateAsync(item);
                }
                production.Deleted = true;
                await _unitOfWork.Repository<Production>().UpdateAsync(production);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");


            return production.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var productions = _unitOfWork.Repository<Production>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await productions.Where(x => x.Status == (int)ProductionStatus.Pending).CountAsync(),
                CheckedCount = await productions.Where(x => x.Status == (int)ProductionStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbProduction = await _unitOfWork.Repository<Production>().FindAsync(id);
            if (dbProduction.Status == (int)ProductionStatus.Pending)
            {
                dbProduction.Status = (int)ProductionStatus.Checked;
                dbProduction.CheckedBy = _workContext.GetUserName();
                //update Production
                await _unitOfWork.Repository<Production>().UpdateAsync(dbProduction);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbProduction.Id
                       && x.Status == (int)ProductionStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbProduction.Id
                    , "/production/production/" + dbProduction.Id, Permissions.Productions.Approve,
                    "Production " + dbProduction.ProductionNo + " is ready for Approval", (int)ProductionStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Production Status has already been checked by " + dbProduction?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<Production>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Production not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Production does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<Production>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)ProductionStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)ProductionStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Production already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbProduction = await _unitOfWork.Repository<Production>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.ProductionDetails));

            dbProduction.Status = (int)ProductionStatus.Approved;
            dbProduction.ApprovedBy = approvedBy;

            #region insert stock and insert transaction
            var adjustmentCostTotal = await AdjustIssuedItemsDuringProduction(dbProduction);
            dbProduction.TotalAdjustmentCost = adjustmentCostTotal;
            await InsertStock(dbProduction);
            await InsertTransaction(dbProduction);
            await ChangeManufacturingOrderStatusAndConsumeStock(dbProduction);
            #endregion
            //update Production
            await _unitOfWork.Repository<Production>().UpdateAsync(dbProduction);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbProduction.Id
                   && x.Status == (int)ProductionStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }
        private async Task<bool> IsDuplicateRequest(Guid productionId)
        {
            var stockInserted = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .AnyAsync(x => x.TransactonType == (int)TransactonType.Production
                       && x.TransactionId == productionId);
            return stockInserted;
        }
        private async Task<Decimal> AdjustIssuedItemsDuringProduction(Production dbProduction)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var manufacturingOrder = _unitOfWork.Repository<ManufacturingOrder>().TableNoTracking().Where(x => x.ManufacturingOrderNo == dbProduction.ManufacturingOrderNo).FirstOrDefault();
            if (manufacturingOrder is null) throw new NotFoundResultException("Associated Manufacturing Order not found");
            decimal adjustmentCostTotal = 0.0M;
            foreach (var item in dbProduction.ProductionDetails)
            {
                var product = await _unitOfWork.Repository<Product>().FindAsync(item.RawMaterialId);
                if (item.AdjustmentQuantity == 0m) continue;
                if (item.AdjustmentQuantity > 0m)
                {
                    // issue stock fifo style
                    var (rmcost, stocks) = _cogsCalculationService.CalculateCOGS(new List<int> { (int)TransactonType.Purchase, (int)TransactonType.TransferReceive, (int)TransactonType.StockAdjustment_Plus }, product, manufacturingOrder.RawMaterialStoreId, item.AdjustmentQuantity);
                    //update production detail adjustment value
                    item.AdjustmentValue = rmcost;
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
                        ProductId = item.RawMaterialId,
                        StoreId = manufacturingOrder.RawMaterialStoreId,
                        OutQty = item.AdjustmentQuantity,
                        OutRate = rmcost / item.AdjustmentQuantity,
                        BatchNo = string.Join(",", stocks.Select(x => x.BatchNo)),
                        TransactionId = dbProduction!.Id,
                        StockInDate = stocks.First().StockInDate,
                        TransactionDate = dbProduction!.ProductionDate,
                        Remark = "Adjust Material during actual production entry" + dbProduction?.FinishedProduct?.Name + " " + dbProduction?.ProductionNo,
                        FinancialYearId = financialYear.Id
                    });
                    adjustmentCostTotal += rmcost;
                }
                else
                {
                    // back to store in issued rate
                    var issuedRate = item.Amount / item.Quantity;
                    var adjustQty = Math.Abs(item.AdjustmentQuantity);
                    if (adjustQty > item.Quantity) throw new BadRequestException("Invalid Adjust Quantity !!");
                    //update production detail adjustment value
                    item.AdjustmentValue = -(issuedRate * adjustQty);
                    await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                    {
                        Id = Guid.NewGuid(),
                        TransactonType = (int)TransactonType.StockAdjustment_Plus,
                        InventoryTypeId = product.InventoryTypeId,
                        ProductTypeId = product.ProductTypeId,
                        ProductId = item.RawMaterialId,
                        StoreId = manufacturingOrder.RawMaterialStoreId,
                        AvailableQty = adjustQty,
                        InQty = adjustQty,
                        InRate = issuedRate,
                        InTransportCost = 0m,
                        BatchNo = dbProduction!.ProductionNo,
                        TransactionId = dbProduction.Id,
                        StockInDate = dbProduction.ProductionDate,
                        TransactionDate = dbProduction.ProductionDate,
                        Remark = "Rest Material Back to store after actual production: " + dbProduction?.ProductionNo,
                        FinancialYearId = financialYear.Id
                    });
                    adjustmentCostTotal -= (issuedRate * adjustQty);
                }
                //// finally update production detail
                await _unitOfWork.Repository<ProductionDetail>().UpdateAsync(item);
            }
            return adjustmentCostTotal;
        }

        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<Production>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Production Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Production does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)ProductionStatus.Checked && dbInfo.Status != (int)ProductionStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)ProductionStatus.Checked
                ? (int)ProductionStatus.Pending
                : (int)ProductionStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<Production>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbProduction = await _unitOfWork.Repository<Production>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.ProductionDetails));
            dbProduction.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)ProductionStatus.Checked:
                    dbProduction.CheckedBy = "";
                    break;
                case (int)ProductionStatus.Approved:
                    dbProduction.ApprovedBy = "";
                    dbProduction.TotalAdjustmentCost = 0;
                    foreach (var item in dbProduction.ProductionDetails)
                    {
                        item.AdjustmentValue = 0;
                        await _unitOfWork.Repository<ProductionDetail>().UpdateAsync(item);
                    }
                    #region Delete Related Entity
                    await DeleteTransaction(dbProduction.ProductionNo);
                    await ReverseStock(dbProduction);
                    await ReverseManufacturingOrderStatus(dbProduction);
                    #endregion
                    break;
            }

            await _unitOfWork.Repository<Production>().UpdateAsync(dbProduction);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbProduction.Status;
        }

        private async Task InsertStock(Production production)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var finishedProduct = await _unitOfWork.Repository<Product>().FindAsync(production.FinishedProductId);
            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.Production,
                InventoryTypeId = finishedProduct.InventoryTypeId,
                ProductTypeId = finishedProduct.ProductTypeId,
                ProductId = production.FinishedProductId,
                StoreId = production.FgstoreId,
                AvailableQty = production.ActualProductionQuantity,
                InQty = production.ActualProductionQuantity,
                InRate = (production.TotalCost + production.TotalAdjustmentCost) / production.ActualProductionQuantity,
                BatchNo = production.ProductionNo,
                TransactionId = production.Id,
                StockInDate = production.ProductionDate,
                TransactionDate = production.ProductionDate,
                Remark = "Production Entry#" + production.ProductionNo + " " + production.Remark,
                FinancialYearId = financialYear.Id
            });
        }

        private async Task ReverseStock(Production production)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            //remove finished goods
            var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.Production
                       && x.TransactionId == production.Id).ToListAsync();
            foreach (var stock in stocks)
            {
                if (stock.AvailableQty == stock.InQty)
                {
                    await _unitOfWork.Repository<Stock>().DeleteAsync(stock.Id);
                }
                else throw new BadRequestException("Stock Already Occupied ! Not Possible to Unpost");
            }
            //unpost adjusted rm
            var stockAdjustmentMinusStocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                     .Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Minus && x.TransactionId == production.Id).ToListAsync();
            if (stockAdjustmentMinusStocks.Any())
            {
                foreach (var stock in stockAdjustmentMinusStocks)
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
                        TransactionId = production.Id,
                        StockInDate = stock.StockInDate,
                        TransactionDate = DateTime.Now,
                        Remark = "Reverse Adjusted Raw Materials for Unpost Production Entry",
                        IsUnpostedEntry = true,
                        FinancialYearId = financialYear.Id
                    });
                }
                await _unitOfWork.Repository<Stock>().DeleteAsync(stockAdjustmentMinusStocks);
            }
            var stockAdjustmentPlusStocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                     .Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Plus && x.TransactionId == production.Id).ToListAsync();
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

        private async Task InsertTransaction(Production production)
        {
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var finishedGoodsInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.FinishedGoodsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(finishedGoodsInventoryAccountName);
            contraAccountIdsForCredit.Append(AccountHeadConstants.FinishedGoodsInventory);
            var workInProgressInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.WorkInProgressInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(workInProgressInventoryAccountName);
            contraAccountIdsForDebit.Append(AccountHeadConstants.WorkInProgressInventory);
            if (production.TotalAdjustmentCost is not 0m)
            {
                var rawMaterialsInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.RawMaterialsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                if (production.TotalAdjustmentCost > 0m)
                {
                    contraAccountNamesForDebit.Append(", " + rawMaterialsInventoryAccountName);
                    contraAccountIdsForDebit.Append(", " + AccountHeadConstants.RawMaterialsInventory);
                }
                else
                {
                    contraAccountNamesForCredit.Append(", " + rawMaterialsInventoryAccountName);
                    contraAccountIdsForCredit.Append(", " + AccountHeadConstants.RawMaterialsInventory);
                }
            }
            //Transaction Entries start here
            //work in process inventory account credit
            await _accountService.HitAccount(_unitOfWork, null, production.ProductionNo, production.Remark,
                 Guid.Parse(AccountHeadConstants.WorkInProgressInventory), 0, production.TotalCost, production.ProductionDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            //raw materials inventory account credit when adjustment > 0 and debit when adjustment < 0 
            if (production.TotalAdjustmentCost is not 0m)
            {
                if (production.TotalAdjustmentCost > 0m)
                {
                    //Rm Inventory credit
                    await _accountService.HitAccount(_unitOfWork, null, production.ProductionNo, production.Remark,
                 Guid.Parse(AccountHeadConstants.RawMaterialsInventory), 0, production.TotalAdjustmentCost, production.ProductionDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }
                else
                {
                    var adjustmentCost = Math.Abs(production.TotalAdjustmentCost);
                    //Rm inventory debit
                    await _accountService.HitAccount(_unitOfWork, null, production.ProductionNo, production.Remark,
                 Guid.Parse(AccountHeadConstants.RawMaterialsInventory), adjustmentCost, 0, production.ProductionDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                }
            }

            //Finished Good Inventory account debit
            await _accountService.HitAccount(_unitOfWork, null, production.ProductionNo, production.Remark,
                Guid.Parse(AccountHeadConstants.FinishedGoodsInventory), (production.TotalCost + production.TotalAdjustmentCost), 0, production.ProductionDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());


        }

        private async Task DeleteTransaction(string productionNo)
        {
            var production = await _unitOfWork.Repository<Production>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.ProductionNo == productionNo);
            if (production is null) throw new NotFoundResultException("production Not Found With this production No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == production.ProductionNo);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }

        }

        private async Task ChangeManufacturingOrderStatusAndConsumeStock(Production production)
        {
            if (!string.IsNullOrWhiteSpace(production.ManufacturingOrderNo))
            {
                var manufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>().TableNoTracking().SingleOrDefaultAsync(x => x.ManufacturingOrderNo == production.ManufacturingOrderNo);
                manufacturingOrder!.Status = (int)ManufacturingOrderStatus.Material_Consumed_In_Production;
                await _unitOfWork.Repository<ManufacturingOrder>().UpdateAsync(manufacturingOrder);
                //stock update
                var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                     .Where(x => x.TransactonType == (int)TransactonType.Issue && x.TransactionId == manufacturingOrder.Id).ToListAsync();
                foreach (var stock in stocks)
                {
                    stock.IsConsumedInProduction = true;
                    await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
                }
            }
        }

        private async Task ReverseManufacturingOrderStatus(Production production)
        {
            if (!string.IsNullOrWhiteSpace(production.ManufacturingOrderNo))
            {
                var manufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>().TableNoTracking().SingleOrDefaultAsync(x => x.ManufacturingOrderNo == production.ManufacturingOrderNo);
                manufacturingOrder!.Status = (int)ManufacturingOrderStatus.Approved;
                await _unitOfWork.Repository<ManufacturingOrder>().UpdateAsync(manufacturingOrder);
                //stock update
                var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                     .Where(x => x.TransactonType == (int)TransactonType.Issue && x.TransactionId == manufacturingOrder.Id).ToListAsync();
                foreach (var stock in stocks)
                {
                    stock.IsConsumedInProduction = false;
                    await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
                }
            }
        }
        public async Task<List<ExpandoObject>> DayWiseProductionSummaryRowColumnDynamic(ProductionRequestModel request)
        {
            List<ExpandoObject> finalResult = new List<ExpandoObject>();
            var queryable = _unitOfWork.Repository<Production>().TableNoTracking().Where(x => x.Status == (int)ProductionStatus.Approved);

            if (request.FromDate.HasValue && request.ToDate.HasValue)
                queryable = queryable.Where(x => x.ProductionDate.Date >= request.FromDate.Value.Date && x.ProductionDate.Date <= request.ToDate.Value.Date);
            if (request.FgstoreId.HasValue)
                queryable = queryable.Where(x => x.FgstoreId == request.FgstoreId.Value);

            var summaryData = await queryable.GroupBy(sd => new { sd.ProductionDate.Date, sd.FinishedProduct.Name })
                                   .Select(g => new
                                   {
                                       Date = g.Key.Date,
                                       ProductName = g.Key.Name.ToCamelCase(),
                                       TotalProduced = g.Sum(sd => sd.ActualProductionQuantity)
                                   }).ToListAsync();
            var uniqueProductNames = summaryData.GroupBy(item => item.ProductName).Select(group => group.Key).OrderBy(x => x).ToList();
            var pivotData = summaryData.GroupBy(x => x.Date).Select(group => new
            {
                Date = group.Key,
                ProductSummaries = group.ToDictionary(item => item.ProductName, item => item.TotalProduced / 1000m)//convert kg to metric ton
            }).OrderBy(x => x.Date).ToList();

            Dictionary<DateTime, Dictionary<string, decimal>> productionsDateWise = new Dictionary<DateTime, Dictionary<string, decimal>>();
            foreach (var pivot in pivotData)
            {
                Dictionary<string, decimal> produced = new Dictionary<string, decimal>();
                foreach (var name in uniqueProductNames)
                {
                    if (pivot.ProductSummaries.ContainsKey(name)) produced[name] = pivot.ProductSummaries[name];
                    else produced[name] = 0;
                }
                productionsDateWise.Add(pivot.Date, produced);
            }
            foreach (var data in productionsDateWise)
            {
                ExpandoObject obj = CreateDynamicObjectFromDictionary(data.Key, data.Value);
                finalResult.Add(obj);
            }
            return finalResult;
        }
        public async Task<List<ExpandoObject>> DayWiseConsumptionQtyRowColumnDynamic(ConsumptionRequestModel request)
        {
            List<ExpandoObject> finalResult = new List<ExpandoObject>();
            var queryable = _unitOfWork.Repository<ProductionDetail>().TableNoTracking().Include(x => x.RawMaterial).Include(x => x.Production).AsQueryable();
            queryable = queryable.Where(x => x.Production.Status >= (int)ProductionStatus.Approved);

            if (request.FromDate.HasValue && request.ToDate.HasValue) queryable = queryable.Where(d => d.Production.ProductionDate.Date >= request.FromDate.Value.ToLocal().Date && d.Production.ProductionDate.Date <= request.ToDate.Value.ToLocal().Date);
            if (request.ProductTypeId.HasValue) queryable = queryable.Where(x => x.RawMaterial.ProductTypeId == request.ProductTypeId.Value);
            var summaryData = await queryable.GroupBy(sd => new { sd.Production.ProductionDate, sd.RawMaterial.Name })
                                   .Select(g => new
                                   {
                                       Date = g.Key.ProductionDate,
                                       ProductName = g.Key.Name.ToCamelCase(),
                                       TotalConsumed = g.Sum(sd => sd.ActualUsedQuantity)
                                   }).ToListAsync();
            var uniqueProductNames = summaryData.GroupBy(item => item.ProductName).Select(group => group.Key).OrderBy(x => x).ToList();
            var pivotData = summaryData.GroupBy(x => x.Date).Select(group => new
            {
                Date = group.Key,
                ProductSummaries = group.ToDictionary(item => item.ProductName, item => item.TotalConsumed)
            }).OrderBy(x => x.Date).ToList();

            Dictionary<DateTime, Dictionary<string, decimal>> consumptionsDateWise = new Dictionary<DateTime, Dictionary<string, decimal>>();
            foreach (var pivot in pivotData)
            {
                Dictionary<string, decimal> produced = new Dictionary<string, decimal>();
                foreach (var name in uniqueProductNames)
                {
                    if (pivot.ProductSummaries.ContainsKey(name)) produced[name] = pivot.ProductSummaries[name];
                    else produced[name] = 0;
                }
                consumptionsDateWise.Add(pivot.Date, produced);
            }
            foreach (var data in consumptionsDateWise)
            {
                ExpandoObject obj = CreateDynamicObjectFromDictionary(data.Key, data.Value);
                finalResult.Add(obj);
            }
            return finalResult;
        }
        public async Task<List<ExpandoObject>> DayWiseConsumptionRateRowColumnDynamic(ConsumptionRequestModel request)
        {
            List<ExpandoObject> finalResult = new List<ExpandoObject>();
            var queryable = _unitOfWork.Repository<ProductionDetail>().TableNoTracking().Include(x => x.RawMaterial).Include(x => x.Production).AsQueryable();
            queryable = queryable.Where(x => x.Production.Status >= (int)ProductionStatus.Approved);

            if (request.FromDate.HasValue && request.ToDate.HasValue) queryable = queryable.Where(d => d.Production.ProductionDate.Date >= request.FromDate.Value.ToLocal().Date && d.Production.ProductionDate.Date <= request.ToDate.Value.ToLocal().Date);
            if (request.ProductTypeId.HasValue) queryable = queryable.Where(x => x.RawMaterial.ProductTypeId == request.ProductTypeId.Value);
            var summaryData = await queryable.GroupBy(sd => new { sd.Production.ProductionDate, sd.RawMaterial.Name })
                                   .Select(g => new
                                   {
                                       Date = g.Key.ProductionDate,
                                       ProductName = g.Key.Name.ToCamelCase(),
                                       AverageConsumptionRate = g.Average(sd => sd.ActualUsedQuantity != 0 ? ((sd.Amount + sd.AdjustmentValue) / sd.ActualUsedQuantity) : 0)
                                   }).ToListAsync();
            var uniqueProductNames = summaryData.GroupBy(item => item.ProductName).Select(group => group.Key).OrderBy(x => x).ToList();
            var pivotData = summaryData.GroupBy(x => x.Date).Select(group => new
            {
                Date = group.Key,
                ProductSummaries = group.ToDictionary(item => item.ProductName, item => item.AverageConsumptionRate)
            }).OrderBy(x => x.Date).ToList();

            Dictionary<DateTime, Dictionary<string, decimal>> consumptionsDateWise = new Dictionary<DateTime, Dictionary<string, decimal>>();
            foreach (var pivot in pivotData)
            {
                Dictionary<string, decimal> produced = new Dictionary<string, decimal>();
                foreach (var name in uniqueProductNames)
                {
                    if (pivot.ProductSummaries.ContainsKey(name)) produced[name] = pivot.ProductSummaries[name];
                    else produced[name] = 0;
                }
                consumptionsDateWise.Add(pivot.Date, produced);
            }
            foreach (var data in consumptionsDateWise)
            {
                ExpandoObject obj = CreateDynamicObjectFromDictionaryForConsumptionRate(data.Key, data.Value);
                finalResult.Add(obj);
            }
            return finalResult;
        }
        private ExpandoObject CreateDynamicObjectFromDictionary(DateTime key, Dictionary<string, decimal> dictionary)
        {
            dynamic customObject = new ExpandoObject();
            var customObjectDictionary = (IDictionary<string, object>)customObject;
            customObjectDictionary["Date"] = key.ToString("dd/MM/yyyy");
            decimal summation = 0m;
            foreach (var kvp in dictionary)
            {
                customObjectDictionary[kvp.Key] = kvp.Value;
                summation += kvp.Value;
            }
            customObjectDictionary["Total"] = summation;
            return customObject;
        }
        private ExpandoObject CreateDynamicObjectFromDictionaryForConsumptionRate(DateTime key, Dictionary<string, decimal> dictionary)
        {
            dynamic customObject = new ExpandoObject();
            var customObjectDictionary = (IDictionary<string, object>)customObject;
            customObjectDictionary["Date"] = key.ToString("dd/MM/yyyy");
            decimal summation = 0m;
            int count = 0;

            foreach (var kvp in dictionary)
            {
                customObjectDictionary[kvp.Key] = kvp.Value;
                summation += kvp.Value;
                if (kvp.Value > 0m) count++;
            }
            if (count > 0)
            {
                decimal average = summation / count;
                customObjectDictionary["Average"] = average;
            }
            else
            {
                // Handle the case when the dictionary is empty to avoid division by zero
                customObjectDictionary["Average"] = 0m;
            }
            return customObject;
        }

        public virtual async Task<Dictionary<string, List<ExpandoObject>>> SalesMoWise(ProductionRequestModel request)
        {
            Dictionary<string, List<ExpandoObject>> finalResult = new Dictionary<string, List<ExpandoObject>>();
            var queryable = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(x => x.Status >= 3);

            if (request.FromDate.HasValue && request.ToDate.HasValue)
                queryable = queryable.Where(x => x.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && x.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);
            //search by zone/zonal chief (to do)

            var queryable1 = from saleInvoice in queryable
                             join saleInvoiceDetail in _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.SaleInvoice) on saleInvoice.Id equals saleInvoiceDetail.SaleInvoiceId
                             join product in _unitOfWork.Repository<Product>().TableNoTracking() on saleInvoiceDetail.ProductId equals product.Id
                             join customer in _unitOfWork.Repository<Account>().TableNoTracking() on saleInvoice.CustomerId equals customer.Id
                             join zone in _unitOfWork.Repository<Zone>().TableNoTracking() on customer.CustomerZoneId equals zone.Id
                             //join employee in _unitOfWork.Repository<Employee>().TableNoTracking() on customer.CustomerMarketingOfficerId equals employee.Id
                             group saleInvoiceDetail by new { ZoneName = zone.Name, MoId = saleInvoiceDetail.SaleInvoice.CustomerMarketingOfficerId, productName = product.Name } into g
                             select new
                             {
                                 ZoneName = g.Key.ZoneName,
                                 //Moname = g.Key.MoName,
                                 Moname = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == g.Key.MoId)!.FullName,
                                 ProductName = g.Key.productName.ToCamelCase(),
                                 TotalSold = g.Sum(sd => sd.Quantity) - g.Sum(sd => sd.ReturnQuantity)
                             };
            var list = await queryable1.OrderBy(x => x.Moname).ToListAsync();
            var uniqueProductNames = list.GroupBy(item => item.ProductName).Select(group => group.Key).OrderBy(x => x).ToList();
            foreach (var item in list.GroupBy(x => x.ZoneName))
            {
                List<ExpandoObject> expandoList = new List<ExpandoObject>();
                var pivotData = item.GroupBy(x => x.Moname).Select(group => new
                {
                    MoName = group.Key,
                    ProductSummaries = group.ToDictionary(item => item.ProductName, item => item.TotalSold / 1000m)//convert kg to metric ton
                });

                Dictionary<string, Dictionary<string, decimal>> productionsDateWise = new Dictionary<string, Dictionary<string, decimal>>();
                foreach (var pivot in pivotData)
                {
                    Dictionary<string, decimal> produced = new Dictionary<string, decimal>();
                    foreach (var name in uniqueProductNames)
                    {
                        if (pivot.ProductSummaries.ContainsKey(name)) produced[name] = pivot.ProductSummaries[name];
                        else produced[name] = 0;
                    }
                    productionsDateWise.Add(pivot.MoName, produced);
                }
                foreach (var data in productionsDateWise)
                {
                    ExpandoObject obj = CreateDynamicObjectFromDictionaryForSalesMoWise(data.Key, data.Value);
                    expandoList.Add(obj);
                }
                finalResult.Add(item.Key.ToString(), expandoList);
            }
            return finalResult;
        }

        public virtual async Task<Dictionary<string, List<ExpandoObject>>> SalesCustomerWise(ProductionRequestModel request)
        {
            Dictionary<string, List<ExpandoObject>> finalResult = new Dictionary<string, List<ExpandoObject>>();
            var queryable = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(x => x.Status >= 3);

            if (request.FromDate.HasValue && request.ToDate.HasValue)
                queryable = queryable.Where(x => x.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && x.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);
            if (request.CustomerMarketingOfficerId.HasValue) queryable = queryable.Where(x => x.Customer.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId.Value);
            if (request.CustomerId.HasValue) queryable = queryable.Where(x => x.CustomerId == request.CustomerId.Value);
            //search by marketing officer(to do)
            var queryable1 = from saleInvoice in queryable
                             join saleInvoiceDetail in _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking() on saleInvoice.Id equals saleInvoiceDetail.SaleInvoiceId
                             join product in _unitOfWork.Repository<Product>().TableNoTracking() on saleInvoiceDetail.ProductId equals product.Id
                             join customer in _unitOfWork.Repository<Account>().TableNoTracking() on saleInvoice.CustomerId equals customer.Id
                             join employee in _unitOfWork.Repository<Employee>().TableNoTracking() on customer.CustomerMarketingOfficerId equals employee.Id
                             group saleInvoiceDetail by new { MoName = employee.FullName, customerName = customer.Name, productName = product.Name } into g
                             select new
                             {
                                 Moname = g.Key.MoName,
                                 CustomerName = g.Key.customerName,
                                 ProductName = g.Key.productName.ToCamelCase(),
                                 TotalSold = g.Sum(sd => sd.Quantity) - g.Sum(sd => sd.ReturnQuantity),
                             };
            var list = await queryable1.ToListAsync();
            var uniqueProductNames = list.GroupBy(item => item.ProductName).Select(group => group.Key).OrderBy(x => x).ToList();
            foreach (var item in list.GroupBy(x => x.Moname))
            {
                List<ExpandoObject> expandoList = new List<ExpandoObject>();
                var pivotData = item.GroupBy(x => x.CustomerName).Select(group => new
                {
                    CustomerName = group.Key,
                    ProductSummaries = group.ToDictionary(item => item.ProductName, item => item.TotalSold / 1000m)//convert kg to metric ton
                });
                Dictionary<string, Dictionary<string, decimal>> productionsDateWise = new Dictionary<string, Dictionary<string, decimal>>();
                foreach (var pivot in pivotData)
                {
                    Dictionary<string, decimal> produced = new Dictionary<string, decimal>();
                    foreach (var name in uniqueProductNames)
                    {
                        if (pivot.ProductSummaries.ContainsKey(name)) produced[name] = pivot.ProductSummaries[name];
                        else produced[name] = 0;
                    }
                    productionsDateWise.Add(pivot.CustomerName, produced);
                }
                foreach (var data in productionsDateWise)
                {
                    ExpandoObject obj = CreateDynamicObjectFromDictionaryForSalesCustomerWise(data.Key, data.Value);
                    expandoList.Add(obj);
                }
                finalResult.Add(item.Key.ToString(), expandoList);
            }
            return finalResult;
        }

        private ExpandoObject CreateDynamicObjectFromDictionaryForSalesMoWise(string key, Dictionary<string, decimal> dictionary)
        {
            dynamic customObject = new ExpandoObject();
            var customObjectDictionary = (IDictionary<string, object>)customObject;
            customObjectDictionary["MO"] = key;
            decimal summation = 0m;
            foreach (var kvp in dictionary)
            {
                customObjectDictionary[kvp.Key] = kvp.Value;
                summation += kvp.Value;
            }
            customObjectDictionary["Total"] = summation;
            return customObject;
        }

        private ExpandoObject CreateDynamicObjectFromDictionaryForSalesCustomerWise(string key, Dictionary<string, decimal> dictionary)
        {
            dynamic customObject = new ExpandoObject();
            var customObjectDictionary = (IDictionary<string, object>)customObject;
            customObjectDictionary["Party"] = key;
            decimal summation = 0m;
            foreach (var kvp in dictionary)
            {
                customObjectDictionary[kvp.Key] = kvp.Value;
                summation += kvp.Value;
            }
            customObjectDictionary["Total"] = summation;
            return customObject;
        }


    }
}
