using Application.Core.Entities;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.Reports;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Accounts.PdfAccount
{
    public interface IAccountReportPdfService
    {
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerLedgerProductWiseReportToPdfAsync(MemoryStream stream, IList<CustomerLedgerProductWiseViewModel> list, Account account, string reportTitle, string costCenterName, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintDayWiseAccountLedgerReportToPdfAsync(MemoryStream stream, IList<SubsidiaryLedgerViewModel> list, Account account, string? costCenterName = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportToPdfAsync(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, List<string>? numericColumnsForSum = null, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, string? supplierName = null, string? costCenterName = null, string? customerName = null);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSubsidiaryLedgerReportToPdfAsync(MemoryStream stream, IList<SubsidiaryLedgerViewModel> list, string reportTitleName, Account? account);
        Task<PdfPTable> SupplierLedgerProductWiseReportToPdfAsync(MemoryStream stream, IList<SupplierLedgerProductWiseViewModel> list, string reportTitle, Account account, string? costCenterName, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPaymentCollectionReportToPdfAsync(MemoryStream stream, List<Transaction> transactions, PaymentCollectionRequestModel request, string reportTitle);
    }
}
