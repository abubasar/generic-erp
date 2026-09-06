using Application.Core.Common;
using Application.Core.Enums;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Report.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.ReportApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportDeliveryNoteController : ControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly IDeliveryNotePdfService _deliveryNotePdfService;

        public ReportDeliveryNoteController(IWorkContext workContext, ITenantService tenantService, IDeliveryNotePdfService deliveryNotePdfService)
        {
            _workContext = workContext;
            _tenantService = tenantService;
            _deliveryNotePdfService = deliveryNotePdfService;
        }

        [HttpGet("{deliveryNoteId}")]
        public virtual async Task<IActionResult> DeliveryNote(Guid deliveryNoteId)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "DELIVERY CHALLAN";

                if (tenantData!.BusinessType == (int)BusinessType.Primary)
                    await _deliveryNotePdfService.PrintDeliveryNotePrimaryReportToPdfAsync(stream, deliveryNoteId, tenantData, userName, headerText);
                if (tenantData!.BusinessType == (int)BusinessType.Secondary)
                    await _deliveryNotePdfService.PrintDeliveryNoteSecondaryReportToPdfAsync(stream, deliveryNoteId, tenantData, userName, headerText);

                bytes = stream.ToArray();
                bytes = PdfHelper.AddA5Footer(bytes, userName);
                return File(bytes, MimeTypes.ApplicationPdf);
            }
        }
    }
}
