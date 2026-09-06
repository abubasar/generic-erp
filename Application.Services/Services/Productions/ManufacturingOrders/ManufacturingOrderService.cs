using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Production.ManufacturingOrder;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Production.ManufacturingOrder;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services.Services.Productions.ManufacturingOrders
{
    public class ManufacturingOrderService : BaseService<ManufacturingOrder, ManufacturingOrderCreationDto, ManufacturingOrderUpdateDto, ManufacturingOrderRequestModel, ManufacturingOrderViewModel>, IManufacturingOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly ICogsCalculationService _cogsCalculationService;

        public ManufacturingOrderService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
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

        public async Task<ManufacturingOrderAggregatorModel> PrepareManufacturingOrderAggregatorModel(ManufacturingOrderRequestModel manufacturingOrderRequest)
        {
            var manufacturingOrderQueryable = _unitOfWork.Repository<ManufacturingOrder>().TableNoTracking().Where(manufacturingOrderRequest.GetExpression());
            return new ManufacturingOrderAggregatorModel
            {
                AggregatorProductionQuantity = await manufacturingOrderQueryable.SumAsync(x => x.ProductionQuantity),
                AggregatorTotalRmused = await manufacturingOrderQueryable.SumAsync(x => x.TotalRmused),
                AggregatorRmCost = await manufacturingOrderQueryable.SumAsync(x => x.RmCost),
                AggregatorStandardDirectExpenseAmount = await manufacturingOrderQueryable.SumAsync(x => x.StandardDirectExpenseAmount),
                AggregatorStandardFactoryOverheadAmount = await manufacturingOrderQueryable.SumAsync(x => x.StandardFactoryOverheadAmount),
                AggregatorTotalCost = await manufacturingOrderQueryable.SumAsync(x => x.TotalCost),
            };
        }

        public async Task<ManufacturingOrderViewModel> GetByIdAsync(Guid id)
        {
            var manufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.FinishedProduct).Include(x => x.RawMaterialStore).Include(x => x.ManufacturingOrderDetails.OrderBy(x => x.RawMaterial.Name))
                .ThenInclude(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit));
            if (manufacturingOrder == null) throw new NotFoundResultException("Manufacturing Order Found With this Id");
            return _mapper.Map<ManufacturingOrderViewModel>(manufacturingOrder);
        }

        public new async Task<Guid> AddAsync(ManufacturingOrderCreationDto manufacturingOrderCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            manufacturingOrderCreationDto.ScheduledDate = manufacturingOrderCreationDto.ScheduledDate.ToLocal();
            var manufacturingOrder = _mapper.Map<ManufacturingOrder>(manufacturingOrderCreationDto);
            var count = _unitOfWork.Repository<ManufacturingOrder>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            manufacturingOrder.Id = Guid.NewGuid();
            manufacturingOrder.ManufacturingOrderNo = "MO" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            manufacturingOrder.FinancialYearId = financialYear.Id;
            manufacturingOrder.Status = (int)ManufacturingOrderStatus.Pending;
            var productCostSetup = await _unitOfWork.Repository<ProductCostSetup>().TableNoTracking().FirstOrDefaultAsync(x => x.ProductId == manufacturingOrder.FinishedProductId);
            if (productCostSetup is null) throw new NotFoundResultException("Please Set Standard Production Cost First !!");
            foreach (var item in manufacturingOrder.ManufacturingOrderDetails)
            {
                item.Id = Guid.NewGuid();
                item.ManufacturingOrderId = manufacturingOrder.Id;
                await _unitOfWork.Repository<ManufacturingOrderDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<ManufacturingOrder>().AddAsync(manufacturingOrder);

            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, manufacturingOrder.Id
                , "/production/manufacturing-order/" + manufacturingOrder.Id, Permissions.ManufacturingOrders.Check,
                "Manufacturing Order " + manufacturingOrder.ManufacturingOrderNo + " is ready for Check", (int)ManufacturingOrderStatus.Pending);

            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return manufacturingOrder.Id;
        }

        public new async Task<Guid> UpdateAsync(ManufacturingOrderUpdateDto manufacturingOrderUpdateDto)
        {
            var dbManufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>().FindAsync(manufacturingOrderUpdateDto.Id);
            //when date updated,convert it to local date
            if (manufacturingOrderUpdateDto.ScheduledDate.Equals(dbManufacturingOrder.ScheduledDate) == false)
            {
                manufacturingOrderUpdateDto.ScheduledDate = manufacturingOrderUpdateDto.ScheduledDate.ToLocal();
            }
            if (dbManufacturingOrder == null) throw new NotFoundResultException("ManufacturingOrder Not Found With this Id");
            var manufacturingOrder = _mapper.Map(manufacturingOrderUpdateDto, dbManufacturingOrder);
            if (dbManufacturingOrder.Status == (int)ManufacturingOrderStatus.Pending || dbManufacturingOrder.Status == (int)ManufacturingOrderStatus.Checked)
            {
                await _unitOfWork.Repository<ManufacturingOrder>().UpdateAsync(manufacturingOrder);
                foreach (var item in manufacturingOrder.ManufacturingOrderDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.ManufacturingOrderId = manufacturingOrder.Id;
                        await _unitOfWork.Repository<ManufacturingOrderDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<ManufacturingOrderDetail>().UpdateAsync(item);
                }
                //Delete for ManufacturingOrderDetails
                if (!string.IsNullOrEmpty(manufacturingOrderUpdateDto.DeletedManufacturingOrderDetailIds))
                {
                    foreach (var id in manufacturingOrderUpdateDto.DeletedManufacturingOrderDetailIds.Split(',').Where(x => x != ""))
                    {
                        var manufacturingOrderDetail = await _unitOfWork.Repository<ManufacturingOrderDetail>().FindAsync(new Guid(id));
                        manufacturingOrderDetail.Deleted = true;
                        await _unitOfWork.Repository<ManufacturingOrderDetail>().UpdateAsync(manufacturingOrderDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");

            return manufacturingOrder.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var manufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>().FindAsync(x => x.Id == id, x => x.Include(x => x.ManufacturingOrderDetails));
            if (manufacturingOrder == null) throw new NotFoundResultException("Manufacturing Order Not Found With this Id");
            if (manufacturingOrder.Status == (int)ManufacturingOrderStatus.Pending)
            {
                foreach (var item in manufacturingOrder.ManufacturingOrderDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<ManufacturingOrderDetail>().UpdateAsync(item);
                }
                manufacturingOrder.Deleted = true;
                await _unitOfWork.Repository<ManufacturingOrder>().UpdateAsync(manufacturingOrder);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");

            return manufacturingOrder.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var manufacturingOrders = _unitOfWork.Repository<ManufacturingOrder>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await manufacturingOrders.Where(x => x.Status == (int)ManufacturingOrderStatus.Pending).CountAsync(),
                CheckedCount = await manufacturingOrders.Where(x => x.Status == (int)ManufacturingOrderStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbManufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>().FindAsync(id);
            if (dbManufacturingOrder.Status == (int)ManufacturingOrderStatus.Pending)
            {
                dbManufacturingOrder.Status = (int)ManufacturingOrderStatus.Checked;
                dbManufacturingOrder.CheckedBy = _workContext.GetUserName();
                //update ManufacturingOrder
                await _unitOfWork.Repository<ManufacturingOrder>().UpdateAsync(dbManufacturingOrder);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbManufacturingOrder.Id
                       && x.Status == (int)ManufacturingOrderStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbManufacturingOrder.Id
                    , "/production/manufacturing-order/" + dbManufacturingOrder.Id, Permissions.ManufacturingOrders.Approve,
                    "Manufacturing Order " + dbManufacturingOrder.ManufacturingOrderNo + " is ready for Approval", (int)ManufacturingOrderStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Manufacturing Order Status has already been checked by " + dbManufacturingOrder?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<ManufacturingOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Manufacturing Order not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Manufacturing Order does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<ManufacturingOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)ManufacturingOrderStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)ManufacturingOrderStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Manufacturing Order already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbManufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.FinishedProduct).Include(x => x.ManufacturingOrderDetails));

            dbManufacturingOrder.Status = (int)ManufacturingOrderStatus.Approved;
            dbManufacturingOrder.ApprovedBy = approvedBy;

            #region Insert Stock and Insert Transaction
            decimal rmCostTotal = 0.0M;
            foreach (var item in dbManufacturingOrder.ManufacturingOrderDetails)
            {
                decimal rmCost = await IssueStock(dbManufacturingOrder, item);
                item.Amount = rmCost;
                //update Manufacturing Order Detail
                await _unitOfWork.Repository<ManufacturingOrderDetail>().UpdateAsync(item);
                rmCostTotal += rmCost;
            }
            var productCostSetup = await _unitOfWork.Repository<ProductCostSetup>().TableNoTracking().FirstOrDefaultAsync(x => x.ProductId == dbManufacturingOrder.FinishedProductId);
            if (productCostSetup is null) throw new BadRequestException("Please Set Standard Production Cost First !!");
            if (productCostSetup is not null)
            {
                var standardDirectExpensesAmount = productCostSetup.DirectExpense * dbManufacturingOrder.ProductionQuantity;
                var standardFactoryOverheadAmount = productCostSetup.FactoryOverhead * dbManufacturingOrder.ProductionQuantity;
                dbManufacturingOrder.RmCost = rmCostTotal;
                dbManufacturingOrder.StandardDirectExpenseAmount = standardDirectExpensesAmount;
                dbManufacturingOrder.StandardFactoryOverheadAmount = standardFactoryOverheadAmount;
                dbManufacturingOrder.TotalCost = rmCostTotal + standardDirectExpensesAmount + standardFactoryOverheadAmount;
                await InsertTransaction(dbManufacturingOrder);
            }
            #endregion

            //update ManufacturingOrder
            await _unitOfWork.Repository<ManufacturingOrder>().UpdateAsync(dbManufacturingOrder);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbManufacturingOrder.Id
                   && x.Status == (int)ManufacturingOrderStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }
        private async Task<bool> IsDuplicateRequest(Guid manufacturingOrderId)
        {
            var stockInserted = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .AnyAsync(x => x.TransactonType == (int)TransactonType.Issue
                       && x.TransactionId == manufacturingOrderId);
            return stockInserted;
        }
        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<ManufacturingOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("ManufacturingOrder Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Manufacturing Order does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)ManufacturingOrderStatus.Checked && dbInfo.Status != (int)ManufacturingOrderStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)ManufacturingOrderStatus.Checked
                ? (int)ManufacturingOrderStatus.Pending
                : (int)ManufacturingOrderStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<ManufacturingOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbManufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.FinishedProduct).Include(x => x.ManufacturingOrderDetails));
            dbManufacturingOrder.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)ManufacturingOrderStatus.Checked:
                    dbManufacturingOrder.CheckedBy = "";
                    break;
                case (int)ManufacturingOrderStatus.Approved:
                    dbManufacturingOrder.ApprovedBy = "";
                    #region Delete Related Entity
                    await DeleteTransaction(dbManufacturingOrder.ManufacturingOrderNo);
                    await ReverseStock(dbManufacturingOrder);
                    #endregion
                    break;
            }

            await _unitOfWork.Repository<ManufacturingOrder>().UpdateAsync(dbManufacturingOrder);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbManufacturingOrder.Status;
        }

        private async Task<decimal> IssueStock(ManufacturingOrder manufacturingOrder, ManufacturingOrderDetail item)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var product = await _unitOfWork.Repository<Product>().FindAsync(item.RawMaterialId);
            var (rmcost, stocks) = _cogsCalculationService.CalculateCOGS(new List<int> { (int)TransactonType.Purchase, (int)TransactonType.StockAdjustment_Plus, (int)TransactonType.TransferReceive }, product, manufacturingOrder.RawMaterialStoreId, item.Quantity);
            foreach (var stock in stocks)
            {
                await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
            }

            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.Issue,
                InventoryTypeId = product.InventoryTypeId,
                ProductTypeId = product.ProductTypeId,
                ProductId = item.RawMaterialId,
                StoreId = manufacturingOrder.RawMaterialStoreId,
                OutQty = item.Quantity,
                OutRate = rmcost / item.Quantity,
                BatchNo = string.Join(",", stocks.Select(x => x.BatchNo)),
                TransactionId = manufacturingOrder.Id,
                StockInDate = stocks.First().StockInDate,
                TransactionDate = manufacturingOrder.ScheduledDate,
                Remark = "Material Issue for Production " + manufacturingOrder?.FinishedProduct?.Name + " " + manufacturingOrder?.ManufacturingOrderNo,
                FinancialYearId = financialYear.Id
            });
            return rmcost;
        }

        private async Task ReverseStock(ManufacturingOrder manufacturingOrder)
        {

            var financialYear = _workContext.GetCurrentFinancialYear();
            var stocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.Issue && x.TransactionId == manufacturingOrder.Id).ToListAsync();
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
                        TransactionId = manufacturingOrder.Id,
                        StockInDate = stock.StockInDate,
                        TransactionDate = DateTime.Now,
                        Remark = "Reverse Stock for manufacturing Order#" + manufacturingOrder.ManufacturingOrderNo + " Unposted",
                        IsUnpostedEntry = true,
                        FinancialYearId = financialYear.Id

                    });
                }
                await _unitOfWork.Repository<Stock>().DeleteAsync(stocks);
            }
        }

        private async Task InsertTransaction(ManufacturingOrder manufacturingOrder)
        {
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var workInProgressInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.WorkInProgressInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(workInProgressInventoryAccountName);
            contraAccountIdsForCredit.Append(AccountHeadConstants.WorkInProgressInventory);
            var rawMaterialsInventoryAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.RawMaterialsInventory.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            var directExpensesStandardAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.DirectExpenses_Standard.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            var factoryOverheadStandardAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.FactoryOverhead_Standard.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(rawMaterialsInventoryAccountName);
            contraAccountIdsForDebit.Append(AccountHeadConstants.RawMaterialsInventory);
            contraAccountNamesForDebit.Append(", " + directExpensesStandardAccountName);
            contraAccountIdsForDebit.Append(", " + AccountHeadConstants.DirectExpenses_Standard);
            contraAccountNamesForDebit.Append(", " + factoryOverheadStandardAccountName);
            contraAccountIdsForDebit.Append(", " + AccountHeadConstants.FactoryOverhead_Standard);
            //Transaction Entries start here
            //work in process inventory account debit
            await _accountService.HitAccount(_unitOfWork, null, manufacturingOrder.ManufacturingOrderNo, manufacturingOrder.Remark,
             Guid.Parse(AccountHeadConstants.WorkInProgressInventory), manufacturingOrder.TotalCost, 0, manufacturingOrder.ScheduledDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            //RM Inventory account credit
            await _accountService.HitAccount(_unitOfWork, null, manufacturingOrder.ManufacturingOrderNo, manufacturingOrder.Remark,
                Guid.Parse(AccountHeadConstants.RawMaterialsInventory), 0, manufacturingOrder.RmCost, manufacturingOrder.ScheduledDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            //Direct Expenses (standard) account credit
            await _accountService.HitAccount(_unitOfWork, null, manufacturingOrder.ManufacturingOrderNo, manufacturingOrder.Remark,
                Guid.Parse(AccountHeadConstants.DirectExpenses_Standard), 0, manufacturingOrder.StandardDirectExpenseAmount, manufacturingOrder.ScheduledDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
            //Factory Overhead (standard) account credit
            await _accountService.HitAccount(_unitOfWork, null, manufacturingOrder.ManufacturingOrderNo, manufacturingOrder.Remark,
                Guid.Parse(AccountHeadConstants.FactoryOverhead_Standard), 0, manufacturingOrder.StandardFactoryOverheadAmount, manufacturingOrder.ScheduledDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

        }

        private async Task DeleteTransaction(string manufacturingOrderNo)
        {
            var manufacturingOrder = await _unitOfWork.Repository<ManufacturingOrder>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.ManufacturingOrderNo == manufacturingOrderNo);
            if (manufacturingOrder is null) throw new NotFoundResultException("Manufacturing Order Not Found With this Manufacturing Order No");
            var transactions = await _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == manufacturingOrder.ManufacturingOrderNo).ToListAsync();
            await _unitOfWork.Repository<Transaction>().DeleteAsync(transactions);
        }

    }
}
