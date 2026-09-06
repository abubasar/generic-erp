using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Inventory.StockTransfer;
using Application.Services.SearchRequestModels.Inventory;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Inventory;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Inventory.StockTransfers
{
    public class StockTransferService : BaseService<StockTransfer, StockTransferCreationDto, StockTransferUpdateDto, StockTransferRequestModel, StockTransferViewModel>, IStockTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly ICogsCalculationService _cogsCalculationService;

        public StockTransferService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
            IHubContext<BroadcastHub, IHubClient> hubContext, ICogsCalculationService cogsCalculationService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _cogsCalculationService = cogsCalculationService;
        }
        public async Task<StockTransferViewModel> GetByIdAsync(Guid id)
        {
            var stockTransfer = await _unitOfWork.Repository<StockTransfer>().FindAsync(x => x.Id == id, x => x.Include(x => x.Source).Include(x => x.Destination)
                .Include(x => x.StockTransferDetails).ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (stockTransfer == null) throw new NotFoundResultException("Stock Transfer Not Found With this Id");
            return _mapper.Map<StockTransferViewModel>(stockTransfer);
        }
        public new async Task<Guid> AddAsync(StockTransferCreationDto stockTransferCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            stockTransferCreationDto.TransferDate = stockTransferCreationDto.TransferDate.ToLocal();
            var stockTransfer = _mapper.Map<StockTransfer>(stockTransferCreationDto);
            var count = _unitOfWork.Repository<StockTransfer>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            stockTransfer.Id = Guid.NewGuid();
            stockTransfer.TransferNo = "ST" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            stockTransfer.FinancialYearId = financialYear.Id;
            stockTransfer.Status = (int)StockTransferStatus.Pending;

            foreach (var item in stockTransfer.StockTransferDetails)
            {
                item.Id = Guid.NewGuid();
                item.StockTransferId = stockTransfer.Id;
                await _unitOfWork.Repository<StockTransferDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<StockTransfer>().AddAsync(stockTransfer);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, stockTransfer.Id
                , "/sales/stock-transfer/" + stockTransfer.Id, Permissions.StockTransfers.Check,
                "Stock Transfer " + stockTransfer.TransferNo + " is ready for Check", (int)StockTransferStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return stockTransfer.Id;
        }
        public new async Task<Guid> UpdateAsync(StockTransferUpdateDto stockTransferUpdateDto)
        {
            var dbStockTransfer = await _unitOfWork.Repository<StockTransfer>().FindAsync(stockTransferUpdateDto.Id);
            //when date updated,convert it to local date
            if (stockTransferUpdateDto.TransferDate.Equals(dbStockTransfer.TransferDate) == false)
            {
                stockTransferUpdateDto.TransferDate = stockTransferUpdateDto.TransferDate.ToLocal();
            }
            if (dbStockTransfer == null) throw new NotFoundResultException("StockTransfer Not Found With this Id");
            var stockTransfer = _mapper.Map(stockTransferUpdateDto, dbStockTransfer);
            if (dbStockTransfer.Status == (int)StockTransferStatus.Pending || dbStockTransfer.Status == (int)StockTransferStatus.Checked)
            {
                await _unitOfWork.Repository<StockTransfer>().UpdateAsync(stockTransfer);
                foreach (var item in stockTransfer.StockTransferDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.StockTransferId = stockTransfer.Id;
                        await _unitOfWork.Repository<StockTransferDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<StockTransferDetail>().UpdateAsync(item);
                }
                //Delete for StockTransferDetails
                if (!string.IsNullOrEmpty(stockTransferUpdateDto.DeletedStockTransferDetailIds))
                {
                    foreach (var id in stockTransferUpdateDto.DeletedStockTransferDetailIds.Split(',').Where(x => x != ""))
                    {
                        var stockTransferDetail = await _unitOfWork.Repository<StockTransferDetail>().FindAsync(new Guid(id));
                        stockTransferDetail.Deleted = true;
                        await _unitOfWork.Repository<StockTransferDetail>().UpdateAsync(stockTransferDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return stockTransfer.Id;
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var stockTransfer = await _unitOfWork.Repository<StockTransfer>().FindAsync(x => x.Id == id, x => x.Include(x => x.StockTransferDetails));
            if (stockTransfer == null) throw new NotFoundResultException("StockTransfer Not Found With this Id");
            if (stockTransfer.Status == (int)StockTransferStatus.Pending)
            {
                foreach (var item in stockTransfer.StockTransferDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<StockTransferDetail>().UpdateAsync(item);
                }
                stockTransfer.Deleted = true;
                await _unitOfWork.Repository<StockTransfer>().UpdateAsync(stockTransfer);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");
            return stockTransfer.Id;
        }
        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var StockTransfers = _unitOfWork.Repository<StockTransfer>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await StockTransfers.Where(x => x.Status == (int)StockTransferStatus.Pending).CountAsync(),
                CheckedCount = await StockTransfers.Where(x => x.Status == (int)StockTransferStatus.Checked).CountAsync()
            };
            return response;
        }
        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbStockTransfer = await _unitOfWork.Repository<StockTransfer>().FindAsync(id);
            if (dbStockTransfer.Status == (int)StockTransferStatus.Pending)
            {
                dbStockTransfer.Status = (int)StockTransferStatus.Checked;
                dbStockTransfer.CheckedBy = _workContext.GetUserName();
                //update StockTransfer
                await _unitOfWork.Repository<StockTransfer>().UpdateAsync(dbStockTransfer);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbStockTransfer.Id
                       && x.Status == (int)StockTransferStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbStockTransfer.Id
                    , "/sales/stock-transfer/" + dbStockTransfer.Id, Permissions.StockTransfers.Approve,
                    "Stock Transfer " + dbStockTransfer.TransferNo + " is ready for Approval", (int)StockTransferStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Stock Transfer Status has already been checked by " + dbStockTransfer?.CheckedBy);
        }
        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<StockTransfer>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Stock Transfer not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Stock Transfer does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<StockTransfer>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)StockTransferStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)StockTransferStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Stock Transfer already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbStockTransfer = await _unitOfWork.Repository<StockTransfer>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.StockTransferDetails).Include(x => x.Source).Include(x => x.Destination));

            dbStockTransfer.Status = (int)StockTransferStatus.Approved;
            dbStockTransfer.ApprovedBy = approvedBy;

            if (dbStockTransfer.Source.InventoryTypeId != dbStockTransfer.Destination.InventoryTypeId)
                throw new BadRequestException("Approval Denied: Source and Destination stores have mismatched inventory types");

            foreach (var item in dbStockTransfer.StockTransferDetails)
            {
                await StockIssueAndReceive(dbStockTransfer, item);
            }
            //update StockTransfer
            await _unitOfWork.Repository<StockTransfer>().UpdateAsync(dbStockTransfer);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbStockTransfer.Id
                   && x.Status == (int)StockTransferStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<StockTransfer>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("StockTransfer Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Stock Transfer does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)StockTransferStatus.Checked && dbInfo.Status != (int)StockTransferStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)StockTransferStatus.Checked
                ? (int)StockTransferStatus.Pending
                : (int)StockTransferStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<StockTransfer>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbStockTransfer = await _unitOfWork.Repository<StockTransfer>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.StockTransferDetails));
            dbStockTransfer.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)StockTransferStatus.Checked:
                    dbStockTransfer.CheckedBy = "";
                    break;
                case (int)StockTransferStatus.Approved:
                    dbStockTransfer.ApprovedBy = "";
                    await ReverseStock(dbStockTransfer);
                    break;
            }

            await _unitOfWork.Repository<StockTransfer>().UpdateAsync(dbStockTransfer);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbStockTransfer.Status;
        }
        private async Task<decimal> StockIssueAndReceive(StockTransfer stockTransfer, StockTransferDetail stockTransferDetail)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var product = await _unitOfWork.Repository<Product>().FindAsync(stockTransferDetail.ProductId);
            var (cogs, stocks) = _cogsCalculationService.CalculateCOGS(new List<int> { (int)TransactonType.Purchase, (int)TransactonType.Production, (int)TransactonType.TransferReceive, (int)TransactonType.StockAdjustment_Plus, (int)TransactonType.SaleReturn }, product, stockTransfer.SourceId, stockTransferDetail.TransferQuantity);
            foreach (var stock in stocks)
            {
                await _unitOfWork.Repository<Stock>().UpdateAsync(stock);
            }
            //transfer issue from source store
            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.TransferIssue,
                InventoryTypeId = product.InventoryTypeId,
                ProductTypeId = product.ProductTypeId,
                ProductId = stockTransferDetail.ProductId,
                StoreId = stockTransfer.SourceId,
                OutQty = stockTransferDetail.TransferQuantity,
                OutRate = cogs / stockTransferDetail.TransferQuantity,
                BatchNo = string.Join(",", stocks.Select(x => x.BatchNo)),
                TransactionId = stockTransfer.Id,
                StockInDate = stocks.First().StockInDate,
                TransactionDate = stockTransfer.TransferDate,
                Remark = "Transfer Issue",
                FinancialYearId = financialYear.Id
            });
            //transfer receive in destination store
            await _unitOfWork.Repository<Stock>().AddAsync(new Stock
            {
                Id = Guid.NewGuid(),
                TransactonType = (int)TransactonType.TransferReceive,
                InventoryTypeId = product.InventoryTypeId,
                ProductTypeId = product.ProductTypeId,
                ProductId = stockTransferDetail.ProductId,
                StoreId = stockTransfer.DestinationId,
                AvailableQty = stockTransferDetail.TransferQuantity,
                InQty = stockTransferDetail.TransferQuantity,
                InRate = cogs / stockTransferDetail.TransferQuantity,
                BatchNo = string.Join(",", stocks.Select(x => x.BatchNo)),
                TransactionId = stockTransfer.Id,
                StockInDate = stockTransfer.TransferDate,
                TransactionDate = stockTransfer.TransferDate,
                Remark = "Transfer Receive",
                FinancialYearId = financialYear.Id
            });
            return cogs;
        }
        private async Task ReverseStock(StockTransfer stockTransfer)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var transferIssuedStocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                      .Where(x => x.TransactonType == (int)TransactonType.TransferIssue && x.TransactionId == stockTransfer.Id).ToListAsync();
            if (transferIssuedStocks.Any())
            {
                foreach (var stock in transferIssuedStocks)
                {
                    await _unitOfWork.Repository<Stock>().AddAsync(new Stock
                    {
                        Id = Guid.NewGuid(),
                        TransactonType = stock.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid() ? (int)TransactonType.Production : (int)TransactonType.Purchase,
                        InventoryTypeId = stock.InventoryTypeId,
                        ProductTypeId = stock.ProductTypeId,
                        ProductId = stock.ProductId,
                        StoreId = stock.StoreId,
                        AvailableQty = stock.OutQty,
                        InQty = stock.OutQty,
                        InRate = stock.OutRate,
                        BatchNo = stock.BatchNo,
                        TransactionId = stockTransfer.Id,
                        StockInDate = stock.StockInDate,
                        TransactionDate = DateTime.Now,
                        Remark = "Reverse Stock for Stock Transfer Unposted",
                        IsUnpostedEntry = true,
                        FinancialYearId = financialYear.Id
                    });
                }
                await _unitOfWork.Repository<Stock>().DeleteAsync(transferIssuedStocks);
            }
            var transferReceivedStocks = await _unitOfWork.Repository<Stock>().TableNoTracking()
                    .Where(x => x.TransactonType == (int)TransactonType.TransferReceive && x.TransactionId == stockTransfer.Id).ToListAsync();
            if (transferReceivedStocks.Any())
            {
                foreach (var stock in transferReceivedStocks)
                {
                    if (stock.AvailableQty == stock.InQty)
                    {
                        await _unitOfWork.Repository<Stock>().DeleteAsync(stock.Id);
                    }
                    else throw new BadRequestException("Stock Already Occupied ! Not Possible to Unpost");
                }
            }

        }


    }
}
