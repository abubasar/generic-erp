using Application.Services.SearchRequestModels.Report;
using Application.Services.ViewModels.Report;
using iTextSharp.text.pdf;
using System.Dynamic;

namespace Application.Services.Services.Report.Pdf
{
    public interface IReportPdfService
    {
        Task<PdfPTable> PrintReportToPdfAsync(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, List<string>? numericColumnsForSum = null, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, Guid? storeId = null, Guid? productId = null, Guid? inventoryTypeId = null, Guid? productTypeId = null);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportForExpandoObjectToPdfAsync(MemoryStream stream, List<object> tableData, string reportTitleName, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, Guid? storeId = null, Guid? productId = null, Guid? inventoryTypeId = null, Guid? productTypeId = null);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportForExpandoObjectForAverageToPdfAsync(MemoryStream stream, List<object> tableData, string reportTitleName, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, Guid? storeId = null, Guid? productId = null, Guid? inventoryTypeId = null, Guid? productTypeId = null);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportForExpandoObject2ToPdfAsync(MemoryStream stream, Dictionary<string, List<ExpandoObject>> tableData, string reportTitleName, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, Guid? storeId = null, Guid? productId = null);
        Task<PdfPTable> PrintItemStockLedgerReportToPdf(MemoryStream stream, List<ItemStockLedger> list, string reportTitleName, StockLedgerRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintFinishedGoodsStockLedgerToPdfAsync(MemoryStream stream, List<FinishedGoodsStockReportViewModel> list, string headerText, FinishedGoodsStockReportRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimaryFinishedGoodsStockLedgerToPdfAsync(MemoryStream stream, List<PrimaryFinishedGoodsStockReportViewModel> list, string headerText, FinishedGoodsStockReportRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimaryStockReportWholeToPdfAsync(MemoryStream stream, List<PrimaryFinishedGoodsStockReportViewModel> list, string headerText, FinishedGoodsStockReportRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintRawMaterialsStockLedgerToPdfAsync(MemoryStream stream, List<RawMaterialsStockReportViewModel> list, string headerText, RawMaterialsStockReportRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintStockDepotProductWiseDetailsReportToPdf(MemoryStream stream, List<StockLedgerReportLine> list, string headerText, StockLedgerRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintStockDepotProductWiseShortReportToPdf(MemoryStream stream, List<StockLedgerReportLine> list, string headerText, StockLedgerRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintFinishedGoodsStockSummaryReportToPdfAsync(MemoryStream stream, List<FinishedGoodsStockReportViewModel> list, string headerText, FinishedGoodsStockReportRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintStockReportToPdfAsync(MemoryStream stream, Tuple<List<StockViewModel>, int> result, string headerText, StockRequestModel request);
    }
}
