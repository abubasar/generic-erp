using Application.Services.ViewModels.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Report.Sales
{
    public interface IDeliveryNotePdfService
    {
        Task PrintDeliveryNotePrimaryReportToPdfAsync(MemoryStream stream, Guid deliveryNoteId, TenantViewModel tenantData, string userName, string headerText);
        Task PrintDeliveryNoteSecondaryReportToPdfAsync(MemoryStream stream, Guid deliveryNoteId, TenantViewModel tenantData, string userName, string headerText);
    }
}
