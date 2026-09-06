using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.ProductAudits;
using Application.Services.Services.Configuration.Tenants;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAuditController : ControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly IProductAuditService _productAuditService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ITenantService _tenantService;

        public ProductAuditController(IWorkContext workContext, IProductAuditService productAuditService, IConfigurationPdfService configurationPdfService, ITenantService tenantService)
        {
            _workContext = workContext;
            _productAuditService = productAuditService;
            _configurationPdfService = configurationPdfService;
            _tenantService = tenantService;
        }

        [Authorize(PrimaryPermissions.ProductAudits.Product_Audit_Report)]
        [HttpPost("product_audit_report/print")]
        public async Task<IActionResult> PrintProductAuditReport(ProductAuditRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }
                var list = await _productAuditService.GetProductAuditData(request);
                var userName = _workContext.GetUserName() ?? "";
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Product Audit Report";
                    var tables = await _configurationPdfService.PrintProductAuditReportToPdfAsync(stream, list, headerText, request);

                    if (request.ReportType == 2)
                    {
                        Guid? tenantId = _workContext.GetTenantId();
                        var tenantData = await _tenantService.GetByIdAsync(tenantId);
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }
    }
}
