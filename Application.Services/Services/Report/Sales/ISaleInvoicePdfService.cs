using Application.Services.ViewModels.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Report.Sales
{
    public interface ISaleInvoicePdfService
    {
        Task PrintSaleInvoicePrimaryReportToPdfAsync(MemoryStream stream, Guid saleInvoiceId, TenantViewModel tenantData, string userName, string headerText);
        Task PrintSaleInvoiceSecondaryReportToPdfAsync(MemoryStream stream, Guid saleInvoiceId, TenantViewModel tenantData, string userName, string headerText);
    }
}
