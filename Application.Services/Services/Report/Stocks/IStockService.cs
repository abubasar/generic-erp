using Application.Services.SearchRequestModels.Accounts;
using Application.Services.SearchRequestModels.Report;
using Application.Services.ViewModels.Accounts.Reports;
using Application.Services.ViewModels.Report;

namespace Application.Services.Services.Report.Stocks
{
    public interface IStockService
    {
        Task<Tuple<List<StockViewModel>, int>> SearchAsync(StockRequestModel request);
        Task<Tuple<List<StockViewModel>, int>> LowStockAsync(StockRequestModel request);
        Task<decimal> GetItemStockAsync(Guid productId, Guid storeId);
        Task<List<StockLedgerReportLine>> PrepareStockLedger(StockLedgerRequestModel request);
        Task<List<FinishedGoodsStockReportViewModel>> PrepareFinishedGoodsStockLedger(FinishedGoodsStockReportRequestModel request);
        Task<List<PrimaryFinishedGoodsStockReportViewModel>> PreparePrimaryFinishedGoodsStockLedger(FinishedGoodsStockReportRequestModel request);
        Task<List<RawMaterialsStockReportViewModel>> PrepareRawMaterialsStockLedger(RawMaterialsStockReportRequestModel request);
        Task<List<CogsStockLedgerReportViewModel>> PrepareStockLedgerTest(CogsCalculationRequestModel request);
        List<ItemStockLedger> PrepareItemStockLedger(StockLedgerRequestModel request);
        Task<List<StockViewModel>> PrepareWorkInProcessInventoryStockAsync();
    }
}
