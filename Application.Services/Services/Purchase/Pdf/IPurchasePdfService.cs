using Application.Core.Entities;
using Application.Services.Dtos.Purchase.PurchaseOrder;
using Application.Services.Dtos.Purchase.PurchaseRequisition;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.SearchRequestModels.Report;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Report.Purchase;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Purchase.Pdf
{
    public interface IPurchasePdfService
    {
        byte[] AddFooter(byte[] bytes);
        Task PrintGoodsReceiveNoteReportToPdfAsync(MemoryStream stream, List<GoodsReceiveNoteViewModel> goodsReceiveNoteViewModels, string reportTitle, bool isDetails, GoodsReceiveNoteRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintGrnItemSupplierWiseReportToPdfAsync(MemoryStream stream, List<GrnItemViewModel> list, string reportTitle, GoodsReceiveNoteRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintGrnSupplierItemWiseReportToPdfAsync(MemoryStream stream, List<GrnItemViewModel> list, string reportTitle, GoodsReceiveNoteRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintGrnTotalQuantityValueAveragePriceReportToPdfAsync(MemoryStream stream, List<GrnItemSummaryViewModel> list, string reportTitle, GoodsReceiveNoteRequestModel request);
        Task PrintLcAdjustmentReportToPdf(MemoryStream stream, List<LcAdjustmentViewModel> lcAdjustmentViewModels, string reportTitle, bool isDetails, LcAdjustmentRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintLcCostEntryAgainstPurchaseOrderReportToPdf(MemoryStream stream, List<LcCostEntryDetailAgainstPoViewModel> list, string headerText, string lcnumber, string ponumber);
        Task PrintLCCostEntryReportToPdf(MemoryStream stream, List<LCCostEntryViewModel> lCCostEntryViewModels, string reportTitle, bool isDetails, LCCostEntryRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductsWithLatestPurchasePriceAsyncReportToPdfAsync(MemoryStream stream, List<ProductWithLatestPriceViewModel> list, string headerText, ProductWithLatestPriceRequestModel request);
        Task<PdfPTable> PrintPurchaseInvoiceItemReportToPdf(MemoryStream stream, List<PurchaseInvoiceItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate);
        Task PrintPurchaseInvoiceReportToPdfAsync(MemoryStream stream, List<PurchaseInvoiceViewModel> purchaseInvoiceViewModels, string reportTitle, bool isDetails, PurchaseInvoiceRequestModel request);
        Task<PdfPTable> PrintPurchaseItemWiseSupplierReportToPdfAsync(MemoryStream stream, List<PurchaseItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate);
        Task PrintPurchaseOrderInvoiceToPdf(MemoryStream stream, PurchaseOrder purchaseOrder, List<PurchaseOrderView> purchaseOrderDetails);
        Task PrintPurchaseOrderSummaryReportToPdf(MemoryStream stream, List<PurchaseOrderViewModel> purchaseOrderViewModels, string reportTitle, bool isDetails, PurchaseOrderRequestModel request);
        Task<PdfPTable> PrintPurchaseReportToPdfAsync(MemoryStream stream, PurchaseReportViewModel list, string reportTitle, DateTime? fromDate, DateTime? toDate);
        //void PrintPurchaseRequisitionInvoiceToPdf(MemoryStream stream, PurchaseRequisition purchaseRequisition, List<PurchaseRequisitionView> purchaseRequisitionDetails);
        void PrintPurchaseRequisitionInvoiceToPdf(MemoryStream stream, Account? supplier, PurchaseRequisition purchaseRequisition, List<PurchaseRequisitionView> purchaseRequisitionDetails, TenantViewModel tenantdata);
        Task PrintPurchaseReturnReportToPdf(MemoryStream stream, List<PurchaseReturnViewModel> purchaseReturnViewModels, string reportTitle, bool isDetails, PurchaseReturnRequestModel request);
        Task<PdfPTable> PrintSupplierWisePurchaseItemReportToPdfAsync(MemoryStream stream, List<PurchaseItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate);
    }
}
