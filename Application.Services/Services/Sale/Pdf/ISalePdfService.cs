using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.SearchRequestModels.Inventory;
using Application.Services.SearchRequestModels.Report;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Inventory;
using Application.Services.ViewModels.Report.Sales;
using Application.Services.ViewModels.Sale;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Sale.Pdf
{
    public interface ISalePdfService
    {
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerMonthlyDiscountReportToPdf(MemoryStream stream, List<CustomerDiscountViewModel> list, string reportTitle, string? customerName, int year, int? month);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerDateWiseDiscountReportToPdf(MemoryStream stream, List<CustomerDiscountViewModel> list, string reportTitle, CustomerDiscountRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesCustomerItemWiseReportToPdf(MemoryStream stream, List<SalesItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate, string? customerName, string? marketingOfficerName, string? storeName, string customerZoneName, string? customerAreaName);
        Task PrintReportToPdf(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, List<string>? numericColumnsForSum = null, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintWarehouseWiseSaleInvoiceReportToPdf(MemoryStream stream, List<SaleInvoiceViewModel> saleInvoiceViewModels, string reportTitleName, SaleInvoiceRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesItemCustomerWiseReportToPdf(MemoryStream stream, List<SalesItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate, string? productName, string? marketingOfficerName, string? storeName, string? customerZoneName, string? customerAreaName);
        Task PrintSaleOrderReportToPdf(MemoryStream stream, List<SaleOrderViewModel> saleOrderViewModels, string reportTitle, bool isDetails, SaleOrderRequestModel request);
        Task PrintDeliveryNoteReportToPdf(MemoryStream stream, List<DeliveryNoteViewModel> deliveryNoteViewModels, string reportTitle, bool isDetails, DeliveryNoteRequestModel request);
        Task PrintSaleReturnReportToPdf(MemoryStream stream, List<SaleReturnViewModel> saleReturnViewModels, string reportTitle, bool isDetails, SaleReturnRequestModel request);
        Task PrintStockTransferReportToPdfAsync(MemoryStream stream, List<StockTransferViewModel> stockTransferViewModels, string reportTitle, bool isDetails, StockTransferRequestModel request);
        Task PrintSaleInvoiceReportToPdf(MemoryStream stream, List<SaleInvoiceViewModel> saleInvoiceViewModels, string reportTitle, bool isDetails, SaleInvoiceRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesReportToPdf(MemoryStream stream, SalesReportViewModel list, string reportTitle, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesTotalMonthWiseReportToPdf(MemoryStream stream, List<SalesTotalMonthWiseViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesTotalDateWiseReportToPdf(MemoryStream stream, List<SalesTotalDateWiseViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSaleTotalProductWiseReportToPdf(MemoryStream stream, List<SalesItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate, string? productName, string? marketingOfficerName, string? storeName, string? customerZoneName, string? customerAreaName);
        Task PrintReceivePaymentReportToPdf(MemoryStream stream, List<ReceivePaymentViewModel> receivePaymentViewModels, string reportTitle, bool isDetails, ReceivePaymentRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerLedgerFeedWiseReportToPdfAsync(MemoryStream stream, List<CustomerLedgerFeedWiseViewModel> list, string headerText, CustomerLedgerFeedWiseRequestModel requestModel);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintTransitSalesReportToPdfAsync(MemoryStream stream, List<SalesOrderItemWithoutDeliveryItemViewModel> list, string headerText, SalesOrderItemRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesOrderHistoryReportToPdfAsync(MemoryStream stream, List<SalesOrderItemWithoutDeliveryItemViewModel> list, string headerText, SalesOrderItemRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimarySaleTotalProductWiseReportToPdf(MemoryStream stream, List<SalesItemDetailViewModel> list, string headerText, SalesItemDetailRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimarySaleTotalProductWiseMultiTerritoryReportToPdf(MemoryStream stream, List<SalesItemDetailViewModel> list, string headerText, SalesItemDetailRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimarySaleTotalProductWiseMultiOfficerReportToPdf(MemoryStream stream, List<SalesItemDetailViewModel> list, string headerText, SalesItemDetailRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimaryCustomerWiseProductWiseSalesQuantityReportToPdf(MemoryStream stream, List<SalesItemDetailViewModel> list, string headerText, SalesItemDetailRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimarySaleAgingReportToPdf(MemoryStream stream, List<SalesAgingReportViewModel> list, string headerText, SalesItemDetailRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimaryTemporarySaleAgingReportToPdf(MemoryStream stream, List<SalesAgingReportViewModel> list, string headerText, SalesItemDetailRequestModel request);
        Task PrintGatePassReportToPdfAsync(MemoryStream stream, Guid deliveryNoteId, string userName, string headerText);
    }
}
