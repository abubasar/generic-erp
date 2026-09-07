using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Enums;
using Application.Core.Industry;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Report.Sales;
using Microsoft.AspNetCore.Mvc;
namespace Application.Api.ReportApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("report")]
    public class ReportSaleReturnController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly IIndustryProfile _industry;
        private readonly ISaleReturnPdfService _saleReturnPdfService;

        public ReportSaleReturnController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService, IIndustryProfile industry, ISaleReturnPdfService saleReturnPdfService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
            _industry = industry;
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

                if (!_industry.Reports.CompactLayout)
                    await _saleReturnPdfService.PrintSaleReturnPrimaryReportToPdfAsync(stream, saleReturnId, tenantData, userName, headerText);
                if (_industry.Reports.CompactLayout)
                    await _saleReturnPdfService.PrintSaleReturnSecondaryReportToPdfAsync(stream, saleReturnId, tenantData, userName, headerText);

                bytes = stream.ToArray();
                bytes = !_industry.Reports.CompactLayout ? PdfHelper.AddFooter(bytes, userName) : PdfHelper.AddA5Footer(bytes, userName);
                return File(bytes, MimeTypes.ApplicationPdf);
            }
        }
    }
}
