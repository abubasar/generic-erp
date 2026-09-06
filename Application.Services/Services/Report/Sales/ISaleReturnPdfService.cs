using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Report.Sales
{
    public interface ISaleReturnPdfService
    {
        Task PrintSaleReturnPrimaryReportToPdfAsync(MemoryStream stream, Guid saleInvoiceId, TenantViewModel tenantData, string userName, string headerText);
        Task PrintSaleReturnSecondaryReportToPdfAsync(MemoryStream stream, Guid saleInvoiceId, TenantViewModel tenantData, string userName, string headerText);
    }
}
