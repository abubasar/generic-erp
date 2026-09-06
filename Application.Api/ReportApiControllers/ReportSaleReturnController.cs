using Application.Core.Common;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Report.Sales;
using Microsoft.AspNetCore.Mvc;
namespace Application.Api.ReportApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportSaleReturnController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly ISaleReturnPdfService _saleReturnPdfService;

        public ReportSaleReturnController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService, ISaleReturnPdfService saleReturnPdfService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
            _saleReturnPdfService = saleReturnPdfService;
        }

        [HttpGet("{saleReturnId}")]
        public virtual async Task<IActionResult> SaleReturns(Guid saleReturnId)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sale Return";

                if (tenantData!.BusinessType == (int)BusinessType.Primary)
                    await _saleReturnPdfService.PrintSaleReturnPrimaryReportToPdfAsync(stream, saleReturnId, tenantData, userName, headerText);
                if (tenantData!.BusinessType == (int)BusinessType.Secondary)
                    await _saleReturnPdfService.PrintSaleReturnSecondaryReportToPdfAsync(stream, saleReturnId, tenantData, userName, headerText);

                bytes = stream.ToArray();
                bytes = tenantData!.BusinessType == (int)BusinessType.Primary ? PdfHelper.AddFooter(bytes, userName) : PdfHelper.AddA5Footer(bytes, userName);
                return File(bytes, MimeTypes.ApplicationPdf);
            }
        }
    }
}
