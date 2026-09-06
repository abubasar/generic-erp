using Application.Services.SearchRequestModels.Accounts;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Configuration;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Configuration.Pdf
{
    public interface IConfigurationPdfService
    {
        Task PrintCustomerReportToPdfAsync(MemoryStream stream, List<CustomerViewModel> customerViewModels, string reportTitleName, CustomerRequestModel request);
        Task PrintCustomerWiseProductDiscountReportToPdf(MemoryStream stream, List<CustomerWiseProductDiscountViewModel> customerWiseProductDiscountViewModels, string reportTitle, CustomerWiseProductDiscountRequestModel request);
        Task PrintReportToPdfAsync(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, bool isLandscape = false);
        Task PrintSupplierReportToPdfAsync(MemoryStream stream, List<SupplierViewModel> supplierViewModels, string reportTitleName);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductPriceListReportToPdf(MemoryStream stream, List<ProductViewModel> list, string headerText, ProductRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductAuditReportToPdfAsync(MemoryStream stream, List<ProductAuditViewModel> list, string headerText, ProductAuditRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductListReportToPdf(MemoryStream stream, List<ProductViewModel> itemList, int businessType, string headerText, ProductRequestModel request);
    }
}
