using Application.Api.Attributes;
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
    [RequiresModule("report")]
    public class ReportSaleInvoiceA5Controller : ControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly ISaleInvoicePdfService _saleInvoicePdfService;

        public ReportSaleInvoiceA5Controller(IWorkContext workContext, ITenantService tenantService, ISaleInvoicePdfService saleInvoicePdfService)
        {
            _workContext = workContext;
            _tenantService = tenantService;
            _saleInvoicePdfService = saleInvoicePdfService;
        }

        [HttpGet("{saleInvoiceId}")]
        public virtual async Task<IActionResult> SaleInvoice(Guid saleInvoiceId)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sale Invoice";

                if (tenantData!.BusinessType == (int)BusinessType.Primary)
                    await _saleInvoicePdfService.PrintSaleInvoicePrimaryReportToPdfAsync(stream, saleInvoiceId, tenantData, userName, headerText);
                if (tenantData!.BusinessType == (int)BusinessType.Secondary)
                    await _saleInvoicePdfService.PrintSaleInvoiceSecondaryReportToPdfAsync(stream, saleInvoiceId, tenantData, userName, headerText);

                bytes = stream.ToArray();
                bytes = tenantData!.BusinessType == (int)BusinessType.Primary ? PdfHelper.AddFooter(bytes, userName) : PdfHelper.AddA5Footer(bytes, userName);
                return File(bytes, MimeTypes.ApplicationPdf);
            }
        }
    }
}
