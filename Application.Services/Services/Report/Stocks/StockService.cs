using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.SearchRequestModels.Report;
using Application.Services.ViewModels.Accounts.Reports;
using Application.Services.ViewModels.Report;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Report.Stocks
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public StockService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public virtual async Task<Tuple<List<StockViewModel>, int>> SearchAsync(StockRequestModel request)
        {
            var query = _unitOfWork.Repository<Stock>().TableNoTracking().GroupBy(x => new { x.StoreId, x.ProductId })
                     .Select(y => new
                     {
                         y.Key.ProductId,
                         y.Key.StoreId,
                         AvailableQty = y.Sum(x => x.AvailableQty),
                         StockValue = y.Sum(x => (x.InRate + x.InTransportCost) * x.AvailableQty)
                     });
            var queryable = from stock in query
                            join store in _unitOfWork.Repository<Store>().TableNoTracking() on stock.StoreId equals store.Id
                            join product in _unitOfWork.Repository<Product>().TableNoTracking() on stock.ProductId equals product.Id
                            join measurementUnit in _unitOfWork.Repository<MeasurementUnit>().TableNoTracking() on product.MeasurementUnitId equals measurementUnit.Id
                            select new StockViewModel()
                            {
                                InventoryTypeId = product.InventoryTypeId,
                                ProductTypeId = product.ProductTypeId,
                                ProductId = stock.ProductId,
                                AlertQuantity = product.AlertQuantity,
                                StoreId = stock.StoreId,
                                AvailableQty = stock.AvailableQty,
                                StockValue = Math.Round(stock.StockValue, 2),
                                SalePriceValue = product.SalePrice * stock.AvailableQty,
                                ProductName = product.Name + "(" + measurementUnit.Name + ")",
                                StoreName = store.Name
                            };
            if (request.StoreId.HasValue) queryable = queryable.Where(x => x.StoreId == request.StoreId);
            if (request.InventoryTypeId.HasValue) queryable = queryable.Where(x => x.InventoryTypeId == request.InventoryTypeId);
            if (request.ProductTypeId.HasValue) queryable = queryable.Where(x => x.ProductTypeId == request.ProductTypeId);
            if (request.ProductId.HasValue) queryable = queryable.Where(x => x.ProductId == request.ProductId);
            var count = queryable.Count();
            if (request.Page != -1) queryable = queryable.Skip(request.Page * request.RowsPerPage).Take(request.RowsPerPage);
            return new Tuple<List<StockViewModel>, int>(await queryable.OrderBy(x => x.ProductName).ToListAsync(), count);

        }

        public virtual async Task<Tuple<List<StockViewModel>, int>> LowStockAsync(StockRequestModel request)
        {
            var query = _unitOfWork.Repository<Stock>().TableNoTracking().GroupBy(x => new { x.StoreId, x.ProductId })
                     .Select(y => new
                     {
                         y.Key.ProductId,
                         y.Key.StoreId,
                         AvailableQty = y.Sum(x => x.AvailableQty),
                         StockValue = y.Sum(x => (x.InRate + x.InTransportCost) * x.AvailableQty)
                     });
            var queryable = from stock in query
                            join store in _unitOfWork.Repository<Store>().TableNoTracking() on stock.StoreId equals store.Id
                            join product in _unitOfWork.Repository<Product>().TableNoTracking() on stock.ProductId equals product.Id
                            join measurementUnit in _unitOfWork.Repository<MeasurementUnit>().TableNoTracking() on product.MeasurementUnitId equals measurementUnit.Id
                            where stock.AvailableQty <= product.AlertQuantity
                            select new StockViewModel()
                            {
                                ProductTypeId = product.ProductTypeId,
                                ProductId = stock.ProductId,
                                AlertQuantity = product.AlertQuantity,
                                StoreId = stock.StoreId,
                                AvailableQty = stock.AvailableQty,
                                StockValue = Math.Round(stock.StockValue, 2),
                                ProductName = product.Name + "(" + measurementUnit.Name + ")",
                                StoreName = store.Name
                            };
            if (request.StoreId.HasValue) queryable = queryable.Where(x => x.StoreId == request.StoreId);
            queryable = queryable.Where(x => x.AvailableQty <= x.AlertQuantity);
            if (request.ProductTypeId.HasValue) queryable = queryable.Where(x => x.ProductTypeId == request.ProductTypeId);
            if (request.ProductId.HasValue) queryable = queryable.Where(x => x.ProductId == request.ProductId);
            var count = queryable.Count();
            if (request.Page != -1) queryable = queryable.Skip(request.Page * request.RowsPerPage).Take(request.RowsPerPage);
            return new Tuple<List<StockViewModel>, int>(await queryable.OrderBy(x => x.ProductName).ToListAsync(), count);

        }

        public virtual async Task<decimal> GetItemStockAsync(Guid productId, Guid storeId)
        {
            var queryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => x.StoreId == storeId && x.ProductId == productId && x.AvailableQty != 0);
            var product = await _unitOfWork.Repository<Product>().FindAsync(productId);
            if (!queryable.Any() && product is null)
            {
                throw new NotFoundResultException("Not Found");
            }
            return queryable.Sum(x => x.AvailableQty);
        }


        public async Task<List<StockLedgerReportLine>> PrepareStockLedger(StockLedgerRequestModel request)
        {
            var queryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry);
            if (request.InventoryTypeId.HasValue) queryable = queryable.Where(x => x.InventoryTypeId == request.InventoryTypeId.Value);
            if (request.ProductTypeId.HasValue) queryable = queryable.Where(x => x.ProductTypeId == request.ProductTypeId.Value);
            var groupedTransactions = queryable.GroupBy(p => new { p.StoreId, p.ProductId }, (key, g) => new { ProductId = key.ProductId, StoreId = key.StoreId, Stocks = g.ToList() });
            if (request.StoreId.HasValue) groupedTransactions = groupedTransactions.Where(x => x.StoreId == request.StoreId.Value);
            if (request.ProductId.HasValue) groupedTransactions = groupedTransactions.Where(x => x.ProductId == request.ProductId.Value);

            List<StockLedgerReportLine> stockItems = new List<StockLedgerReportLine>();
            foreach (var item in groupedTransactions)
            {
                // Initialize the opening quantity and value 
                var openingTransactions = item.Stocks.Where(x => x.TransactionDate.Date < request.FromDate.Date);
                decimal openingQuantity = openingTransactions.Sum(x => x.InQty) - openingTransactions.Sum(x => x.OutQty);
                decimal openingValue = openingTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost)) - openingTransactions.Sum(x => x.OutQty * x.OutRate);
                //this period transactions
                var thisPeriodTransactions = item.Stocks.Where(x => x.TransactionDate.Date >= request.FromDate.Date && x.TransactionDate.Date <= request.ToDate.Date);
                decimal receivedQuantity = thisPeriodTransactions.Sum(x => x.InQty);
                decimal receivedValue = thisPeriodTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost));
                decimal issuedQuantity = thisPeriodTransactions.Sum(x => x.OutQty);
                decimal issuedValue = thisPeriodTransactions.Sum(x => x.OutQty * x.OutRate);

                var product = await _unitOfWork.Repository<Product>().FindAsync(item.ProductId);
                var store = await _unitOfWork.Repository<Store>().FindAsync(item.StoreId);
                stockItems.Add(new StockLedgerReportLine()
                {
                    ItemName = product.Name,
                    StoreName = store.Name,
                    OpeningQty = Math.Round(openingQuantity, 2),
                    OpeningValue = Math.Round(openingValue, 2),
                    InQty = Math.Round(receivedQuantity, 2),
                    InValue = Math.Round(receivedValue, 2),
                    OutQty = Math.Round(issuedQuantity, 2),
                    OutValue = Math.Round(issuedValue, 2),
                    ClosingQty = Math.Round((openingQuantity + receivedQuantity - issuedQuantity), 2),
                    ClosingValue = Math.Round((openingValue + receivedValue - issuedValue), 2)
                });
            }
            return stockItems.OrderBy(x => x.StoreName).ThenBy(x => x.ItemName).ToList();
        }

        public async Task<List<FinishedGoodsStockReportViewModel>> PrepareFinishedGoodsStockLedger(FinishedGoodsStockReportRequestModel request)
        {
            var queryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry);
            if (request.InventoryTypeId.HasValue) queryable = queryable.Where(x => x.InventoryTypeId == request.InventoryTypeId.Value);
            if (request.StoreId.HasValue) queryable = queryable.Where(x => x.StoreId == request.StoreId.Value);
            var groupedTransactions = queryable.GroupBy(p => new { p.StoreId, p.ProductId }, (key, g) => new { ProductId = key.ProductId, StoreId = key.StoreId, Stocks = g.ToList() });

            List<FinishedGoodsStockReportViewModel> finishedGoodsStockItems = new List<FinishedGoodsStockReportViewModel>();

            foreach (var item in groupedTransactions)
            {
                // Initialize the opening quantity and value 
                var openingTransactions = item.Stocks.Where(x => x.TransactionDate.Date < request.FromDate.Date);
                decimal openingQuantity = openingTransactions.Sum(x => x.InQty) - openingTransactions.Sum(x => x.OutQty);
                decimal openingValue = openingTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost)) - openingTransactions.Sum(x => x.OutQty * x.OutRate);
                //this period transactions
                var thisPeriodTransactions = item.Stocks.Where(x => x.TransactionDate.Date >= request.FromDate.Date && x.TransactionDate.Date <= request.ToDate.Date);
                decimal receivedQuantity = thisPeriodTransactions.Sum(x => x.InQty);
                decimal receivedValue = thisPeriodTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost));
                decimal issuedQuantity = thisPeriodTransactions.Sum(x => x.OutQty);
                decimal issuedValue = thisPeriodTransactions.Sum(x => x.OutQty * x.OutRate);
                //InQty
                decimal productionQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.Production).Sum(x => x.InQty);
                decimal transferReceiveQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.TransferReceive).Sum(x => x.InQty);
                decimal saleReturnQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.SaleReturn).Sum(x => x.InQty);
                decimal adjustmentInQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Plus).Sum(x => x.InQty);
                //OutQty
                decimal saleQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.Sale).Sum(x => x.OutQty);
                decimal transferIssueQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.TransferIssue).Sum(x => x.OutQty);
                decimal adjustmentOutQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Minus).Sum(x => x.OutQty);

                var product = await _unitOfWork.Repository<Product>().FindAsync(item.ProductId);
                var store = await _unitOfWork.Repository<Store>().FindAsync(item.StoreId);
                var productType = await _unitOfWork.Repository<ProductType>().FindAsync(product.ProductTypeId);

                finishedGoodsStockItems.Add(new FinishedGoodsStockReportViewModel()
                {
                    ProductName = product.Name,
                    Code = product.Code,
                    ProductTypeName = productType.Name,
                    StoreName = store.Name,
                    BagSize = product.BagWeight,
                    OpeningQty = Math.Round(openingQuantity, 2),
                    OpeningValue = Math.Round(openingValue, 2),
                    ProductionQty = Math.Round(productionQty, 2),
                    TransferReceiveQty = Math.Round(transferReceiveQty, 2),
                    SaleReturnQty = Math.Round(saleReturnQty, 2),
                    AdjustmentInQty = Math.Round(adjustmentInQty, 2),
                    InValue = Math.Round(receivedValue, 2),
                    SaleQty = Math.Round(saleQty, 2),
                    TransferIssueQty = Math.Round(transferIssueQty, 2),
                    AdjustmentOutQty = Math.Round(adjustmentOutQty, 2),
                    OutValue = Math.Round(issuedValue, 2),
                    ClosingQty = Math.Round((openingQuantity + receivedQuantity - issuedQuantity), 2),
                    ClosingValue = Math.Round((openingValue + receivedValue - issuedValue), 2)
                });
            }
            return finishedGoodsStockItems.OrderBy(x => x.ProductName).ToList();
        }
        public async Task<List<PrimaryFinishedGoodsStockReportViewModel>> PreparePrimaryFinishedGoodsStockLedger(FinishedGoodsStockReportRequestModel request)
        {
            var queryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry);
            if (request.InventoryTypeId.HasValue) queryable = queryable.Where(x => x.InventoryTypeId == request.InventoryTypeId.Value);
            if (request.StoreId.HasValue) queryable = queryable.Where(x => x.StoreId == request.StoreId.Value);
            var groupedTransactions = queryable.GroupBy(p => new { p.StoreId, p.ProductId }, (key, g) => new { ProductId = key.ProductId, StoreId = key.StoreId, Stocks = g.ToList() });

            List<PrimaryFinishedGoodsStockReportViewModel> finishedGoodsStockItems = new List<PrimaryFinishedGoodsStockReportViewModel>();

            foreach (var item in groupedTransactions)
            {
                // Initialize the opening quantity and value
                var openingTransactions = item.Stocks.Where(x => x.TransactionDate.Date < request.FromDate.Date);
                decimal openingQuantity = openingTransactions.Sum(x => x.InQty) - openingTransactions.Sum(x => x.OutQty);
                decimal openingValue = openingTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost)) - openingTransactions.Sum(x => x.OutQty * x.OutRate);
                //this period transactions
                var thisPeriodTransactions = item.Stocks.Where(x => x.TransactionDate.Date >= request.FromDate.Date && x.TransactionDate.Date <= request.ToDate.Date);
                decimal receivedQuantity = thisPeriodTransactions.Sum(x => x.InQty);
                decimal receivedValue = thisPeriodTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost));
                decimal issuedQuantity = thisPeriodTransactions.Sum(x => x.OutQty);
                decimal issuedValue = thisPeriodTransactions.Sum(x => x.OutQty * x.OutRate);
                //InQty
                decimal productionQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.Production).Sum(x => x.InQty);
                decimal purchaseQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.Purchase).Sum(x => x.InQty);
                decimal transferReceiveQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.TransferReceive).Sum(x => x.InQty);
                decimal saleReturnQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.SaleReturn).Sum(x => x.InQty);
                decimal adjustmentInQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Plus).Sum(x => x.InQty);
                //OutQty
                decimal saleQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.Sale).Sum(x => x.OutQty);
                decimal purchaseReturnQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.PurchaseReturn).Sum(x => x.OutQty);
                decimal transferIssueQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.TransferIssue).Sum(x => x.OutQty);
                decimal adjustmentOutQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Minus).Sum(x => x.OutQty);
                //BonusQty
                var salesItem = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.SaleInvoice).Where(x => x.SaleInvoice.Status >= (int)SaleInvoiceStatus.Approved && x.SaleInvoice.StoreId == item.StoreId && x.SaleInvoice.InvoiceDate.Date >= request.FromDate.Date && x.SaleInvoice.InvoiceDate.Date <= request.ToDate.Date && x.ProductId == item.ProductId).AsQueryable();
                var saleBonusQty = salesItem.Sum(x => x.BonusQuantity);
                var salesReturnItem = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.SaleReturn).Where(x => x.SaleReturn.Status >= (int)SaleReturnStatus.Approved && x.SaleReturn.StoreId == item.StoreId && x.SaleReturn.SaleReturnDate.Date >= request.FromDate.Date && x.SaleReturn.SaleReturnDate.Date <= request.ToDate.Date && x.ProductId == item.ProductId).AsQueryable();
                var returnBonusQty = salesReturnItem.Sum(x => x.ReturnBonusQuantity);

                var product = await _unitOfWork.Repository<Product>().TableNoTracking().Include(x => x.PackSize).SingleOrDefaultAsync(x => x.Id == item.ProductId);
                var store = await _unitOfWork.Repository<Store>().FindAsync(item.StoreId);
                var productType = await _unitOfWork.Repository<ProductType>().FindAsync(product.ProductTypeId);

                finishedGoodsStockItems.Add(new PrimaryFinishedGoodsStockReportViewModel()
                {
                    ProductName = product.Name,
                    Code = product.Code,
                    ProductTypeName = productType.Name,
                    PackSize = product?.PackSize?.Name,
                    StoreName = store.Name,
                    TradePrice = product!.SalePrice,
                    OpeningQty = Math.Round(openingQuantity, 2),
                    OpeningValue = Math.Round(openingValue, 2),
                    ProductionQty = Math.Round(productionQty, 2),
                    PurchaseQty = Math.Round(purchaseQty, 2),
                    TransferReceiveQty = Math.Round(transferReceiveQty, 2),
                    SaleReturnQty = Math.Round(saleReturnQty, 2),
                    ReturnBonusQty = returnBonusQty,
                    AdjustmentInQty = Math.Round(adjustmentInQty, 2),
                    InValue = Math.Round(receivedValue, 2),
                    SaleQty = Math.Round(saleQty, 2),
                    SaleBonusQty = saleBonusQty,
                    PurchaseRetrunQty = Math.Round(purchaseReturnQty, 2),
                    TransferIssueQty = Math.Round(transferIssueQty, 2),
                    AdjustmentOutQty = Math.Round(adjustmentOutQty, 2),
                    OutValue = Math.Round(issuedValue, 2),
                    ClosingQty = Math.Round((openingQuantity + receivedQuantity - issuedQuantity), 2)
                });
            }
            return finishedGoodsStockItems.OrderBy(x => x.ProductName).ToList();
        }

        public async Task<List<RawMaterialsStockReportViewModel>> PrepareRawMaterialsStockLedger(RawMaterialsStockReportRequestModel request)
        {
            var queryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry);
            if (request.InventoryTypeId.HasValue) queryable = queryable.Where(x => x.InventoryTypeId == request.InventoryTypeId.Value);
            if (request.ProductTypeId.HasValue) queryable = queryable.Where(x => x.ProductTypeId == request.ProductTypeId.Value);
            if (request.StoreId.HasValue) queryable = queryable.Where(x => x.StoreId == request.StoreId.Value);
            var groupedTransactions = queryable.GroupBy(p => p.ProductId, (key, g) => new { ProductId = key, Stocks = g.ToList() });

            List<RawMaterialsStockReportViewModel> rawMaterialsStockItems = new();

            foreach (var item in groupedTransactions)
            {
                // Initialize the opening quantity and value 
                var openingTransactions = item.Stocks.Where(x => x.TransactionDate.Date < request.FromDate.Date);
                decimal openingQuantity = openingTransactions.Sum(x => x.InQty) - openingTransactions.Sum(x => x.OutQty);
                decimal openingValue = openingTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost)) - openingTransactions.Sum(x => x.OutQty * x.OutRate);
                //this period transactions
                var thisPeriodTransactions = item.Stocks.Where(x => x.TransactionDate.Date >= request.FromDate.Date && x.TransactionDate.Date <= request.ToDate.Date);
                decimal receivedQuantity = thisPeriodTransactions.Sum(x => x.InQty);
                decimal receivedValue = thisPeriodTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost));
                decimal issuedQuantity = thisPeriodTransactions.Sum(x => x.OutQty);
                decimal issuedValue = thisPeriodTransactions.Sum(x => x.OutQty * x.OutRate);
                //InQty
                decimal purchaseQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.Purchase).Sum(x => x.InQty);
                decimal adjustmentInQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Plus).Sum(x => x.InQty);
                decimal transferReceiveQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.TransferReceive).Sum(x => x.InQty);
                //OutQty
                decimal issueQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.Issue).Sum(x => x.OutQty);
                decimal transferIssueQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.TransferIssue).Sum(x => x.OutQty);
                decimal purchaseReturnQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.PurchaseReturn).Sum(x => x.OutQty);
                decimal adjustmentOutQty = thisPeriodTransactions.Where(x => x.TransactonType == (int)TransactonType.StockAdjustment_Minus).Sum(x => x.OutQty);

                var product = await _unitOfWork.Repository<Product>().FindAsync(item.ProductId);
                var productType = await _unitOfWork.Repository<ProductType>().FindAsync(product.ProductTypeId);
                var measurementUnit = await _unitOfWork.Repository<MeasurementUnit>().FindAsync(product.MeasurementUnitId);

                rawMaterialsStockItems.Add(new RawMaterialsStockReportViewModel()
                {
                    ProductName = product.Name,
                    Code = product.Code,
                    ProductTypeId = product.ProductTypeId,
                    ProductTypeName = productType.Name,
                    MeasurementUnitName = measurementUnit.Name,
                    OpeningQty = Math.Round(openingQuantity, 6),
                    OpeningValue = Math.Round(openingValue, 6),
                    PurchaseQty = Math.Round(purchaseQty, 6),
                    AdjustmentInQty = Math.Round(adjustmentInQty, 6),
                    TransferReceiveQty = Math.Round(transferReceiveQty, 6),
                    InValue = Math.Round(receivedValue, 2),
                    IssueQty = Math.Round(issueQty, 6),
                    TransferIssueQty = Math.Round(transferIssueQty, 6),
                    PurchaseReturnQty = Math.Round(purchaseReturnQty, 6),
                    AdjustmentOutQty = Math.Round(adjustmentOutQty, 6),
                    OutValue = Math.Round(issuedValue, 2),
                    ClosingQty = Math.Round((openingQuantity + receivedQuantity - issuedQuantity), 3),
                    ClosingValue = Math.Round((openingValue + receivedValue - issuedValue), 2)
                });
            }
            return rawMaterialsStockItems.OrderBy(x => x.ProductName).ToList();
        }

        private async Task<List<StockLedgerReportLine>> PrepareStockLedgerDetails(StockLedgerRequestModel request)
        {
            var groupedTransactions = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry).GroupBy(p => new { p.StoreId, p.ProductId }, (key, g) => new { ProductId = key.ProductId, StoreId = key.StoreId, Stocks = g.ToList() });
            if (request.StoreId.HasValue) groupedTransactions = groupedTransactions.Where(x => x.StoreId == request.StoreId.Value);
            if (request.ProductId.HasValue) groupedTransactions = groupedTransactions.Where(x => x.ProductId == request.ProductId.Value);

            List<StockLedgerReportLine> stockItems = new List<StockLedgerReportLine>();
            foreach (var item in groupedTransactions)
            {
                // Initialize the opening quantity and value 
                var openingTransactions = item.Stocks.Where(x => x.TransactionDate.Date < request.FromDate.Date);
                decimal openingQuantity = openingTransactions.Sum(x => x.InQty) - openingTransactions.Sum(x => x.OutQty);
                decimal openingValue = openingTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost)) - openingTransactions.Sum(x => x.OutQty * x.OutRate);
                //this period transactions
                var thisPeriodTransactions = item.Stocks.Where(x => x.TransactionDate.Date >= request.FromDate.Date && x.TransactionDate.Date <= request.ToDate.Date);


                decimal receivedQuantity = thisPeriodTransactions.Sum(x => x.InQty);
                decimal receivedValue = thisPeriodTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost));
                decimal issuedQuantity = thisPeriodTransactions.Sum(x => x.OutQty);
                decimal issuedValue = thisPeriodTransactions.Sum(x => x.OutQty * x.OutRate);

                var product = await _unitOfWork.Repository<Product>().FindAsync(item.ProductId);
                var store = await _unitOfWork.Repository<Store>().FindAsync(item.StoreId);

                stockItems.Add(new StockLedgerReportLine()
                {
                    ItemName = product.Name,
                    StoreName = store.Name,
                    OpeningQty = Math.Round(openingQuantity, 2),
                    OpeningValue = Math.Round(openingValue, 2),
                    InQty = Math.Round(receivedQuantity, 2),
                    InValue = Math.Round(receivedValue, 2),
                    OutQty = Math.Round(issuedQuantity, 2),
                    OutValue = Math.Round(issuedValue, 2),
                    ClosingQty = Math.Round((openingQuantity + receivedQuantity - issuedQuantity), 2),
                    ClosingValue = Math.Round((openingValue + receivedValue - issuedValue), 2)
                });
            }
            return stockItems.OrderBy(x => x.ItemName).ToList();
        }

        public List<ItemStockLedger> PrepareItemStockLedger(StockLedgerRequestModel request)
        {
            var stockQueryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry);
            if (request.InventoryTypeId.HasValue) stockQueryable = stockQueryable.Where(x => x.InventoryTypeId == request.InventoryTypeId.Value);
            if (request.StoreId.HasValue) stockQueryable = stockQueryable.Where(x => x.StoreId == request.StoreId.Value);
            if (request.ProductId.HasValue) stockQueryable = stockQueryable.Where(x => x.ProductId == request.ProductId.Value);
            var openingStockTransactions = stockQueryable.Where(x => x.TransactionDate.Date < request.FromDate.Date);
            var stockTransactions = stockQueryable.Where(x => x.TransactionDate.Date >= request.FromDate.Date && x.TransactionDate.Date <= request.ToDate.Date).OrderBy(x => x.TransactionDate);


            List<ItemStockLedger> stockItems = new List<ItemStockLedger>();
            var openingStockQty = openingStockTransactions.Sum(x => x.InQty) - openingStockTransactions.Sum(x => x.OutQty);
            var openingStockValue = openingStockTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost)) - openingStockTransactions.Sum(x => x.OutQty * x.OutRate);
            stockItems.Add(new ItemStockLedger
            {
                Date = "",
                Description = "Opening Balance",
                InQty = 0m,
                InRate = 0m,
                OutQty = 0m,
                OutRate = 0m,
                QtyBalance = openingStockQty,
                ValueBalance = openingStockValue,
            });
            foreach (var item in stockTransactions)
            {
                openingStockQty += item.InQty;
                openingStockQty -= item.OutQty;

                openingStockValue += (item.InQty * (item.InRate + item.InTransportCost));
                openingStockValue -= (item.OutQty * item.OutRate);
                stockItems.Add(new ItemStockLedger
                {
                    Date = item.TransactionDate.ToString("dd/MM/yyyy"),
                    Description = item.Remark,
                    InQty = item.InQty,
                    InRate = item.InRate + item.InTransportCost,
                    OutQty = item.OutQty,
                    OutRate = item.OutRate,
                    QtyBalance = openingStockQty,
                    ValueBalance = openingStockValue,
                });
            }

            return stockItems;
        }

        public async Task<List<StockViewModel>> PrepareWorkInProcessInventoryStockAsync()
        {
            var queryable = _unitOfWork.Repository<Stock>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit)
            .Where(x => x.TransactonType == (int)TransactonType.Issue && !x.IsConsumedInProduction)
           .GroupBy(x => x.ProductId).Select(g => new StockViewModel
           {
               ProductId = g.Key,
               ProductName = g.First().Product.Name + "(" + g.First().Product.MeasurementUnit.Name + ")",
               AvailableQty = g.Sum(x => x.OutQty),
               StockValue = Math.Round(g.Sum(x => x.OutRate * x.OutQty), 2)
           }).OrderBy(x => x.ProductName);
            return new List<StockViewModel>(await queryable.OrderBy(x => x.ProductName).ToListAsync());
        }

        //This Method only will be used for calculating COGS in CogsController
        public async Task<List<CogsStockLedgerReportViewModel>> PrepareStockLedgerTest(CogsCalculationRequestModel request)
        {
            var queryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry);
            if (request.InventoryTypeId.HasValue) queryable = queryable.Where(x => x.InventoryTypeId == request.InventoryTypeId.Value);
            var groupedTransactions = queryable.GroupBy(p => new { p.StoreId, p.ProductId }, (key, g) => new { ProductId = key.ProductId, StoreId = key.StoreId, Stocks = g.ToList() });

            List<CogsStockLedgerReportViewModel> stockItems = new List<CogsStockLedgerReportViewModel>();

            foreach (var item in groupedTransactions)
            {
                // Initialize the opening quantity and value 
                var openingTransactions = item.Stocks.Where(x => x.TransactionDate.Date < request.FromDate);
                decimal openingValue = openingTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost)) - openingTransactions.Sum(x => x.OutQty * x.OutRate);
                //this period transactions
                var thisPeriodTransactions = item.Stocks.Where(x => x.TransactionDate.Date >= request.FromDate && x.TransactionDate.Date <= request.ToDate);
                decimal receivedValue = thisPeriodTransactions.Sum(x => x.InQty * (x.InRate + x.InTransportCost));
                decimal issuedValue = thisPeriodTransactions.Sum(x => x.OutQty * x.OutRate);

                var product = await _unitOfWork.Repository<Product>().FindAsync(item.ProductId);

                stockItems.Add(new CogsStockLedgerReportViewModel()
                {
                    ItemName = product.Name,
                    OpeningValue = Math.Round(openingValue, 2),
                    ClosingValue = Math.Round((openingValue + receivedValue - issuedValue), 2)
                });
            }
            return stockItems.OrderBy(x => x.ItemName).ToList();
        }
    }
}
