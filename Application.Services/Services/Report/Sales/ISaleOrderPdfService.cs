using Application.Core.Entities;
using Application.Services.ViewModels.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Report.Sales
{
    public interface ISaleOrderPdfService
    {
        Task PrintSaleOrderPrimaryReportToPdfAsync(MemoryStream stream, SaleOrder? salesOrder, TenantViewModel tenantData, string headerText);
        Task PrintSaleOrderSecondaryReportToPdfAsync(MemoryStream stream, SaleOrder? salesOrder, TenantViewModel tenantData, string headerText);
    }
}
