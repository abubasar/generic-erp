using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Industry;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Report;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Report.Sales;
using Application.Services.Services.Sale.Pdf;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Application.Core.Constants.Permissions;

namespace ErpReport.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("report")]
    public class ReportSaleOrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly IIndustryProfile _industry;
        private readonly ISaleOrderPdfService _saleOrderPdfService;

        public ReportSaleOrderController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService, IIndustryProfile industry, ISaleOrderPdfService saleOrderPdfService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
            _industry = industry;
            _saleOrderPdfService = saleOrderPdfService;
        }


        [HttpGet("{saleOrderId}")]
        public virtual async Task<IActionResult> SaleOrder(Guid saleOrderId)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var salesOrder = await _unitOfWork.Repository<SaleOrder>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Include(x => x.CustomerTerritory).Where(d =>
                                d.Id == saleOrderId).FirstOrDefaultAsync();
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sale Order";

                if (!_industry.Reports.CompactLayout)
                    await _saleOrderPdfService.PrintSaleOrderPrimaryReportToPdfAsync(stream, salesOrder, tenantData, headerText);
                if (_industry.Reports.CompactLayout)
                    await _saleOrderPdfService.PrintSaleOrderSecondaryReportToPdfAsync(stream, salesOrder, tenantData, headerText);

                bytes = stream.ToArray();
                bytes = PdfHelper.AddFooter(bytes, userName);
                return File(bytes, MimeTypes.ApplicationPdf);
            }
        }
    }
}
