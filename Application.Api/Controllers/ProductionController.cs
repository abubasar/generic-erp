using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Interfaces;
using Application.Services.Dtos.Production.Production;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Productions.Pdf;
using Application.Services.Services.Productions.Productions;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Production.Production;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductionService _productionService;
        private readonly IProductionPdfService _productionPdfService;
        private readonly ITenantService _tenantService;

        public ProductionController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IProductionService productionService, IProductionPdfService productionPdfService, ITenantService tenantService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _productionService = productionService;
            _productionPdfService = productionPdfService;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.Productions.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ProductionViewModel>, int>>> Search(ProductionRequestModel request)
        {
            return await Result<Tuple<List<ProductionViewModel>, int>>.SuccessAsync(await _productionService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Productions.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<ProductionAggregatorModel>> ReportAggregates(ProductionRequestModel request)
        {
            return await Result<ProductionAggregatorModel>.SuccessAsync(await _productionService.PrepareProductionAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.Productions.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<ProductionViewModel>> GetById(Guid id)
        {
            return await Result<ProductionViewModel>.SuccessAsync(await _productionService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.Productions.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _productionService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.Productions.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ProductionCreationDto productionCreationDto)
        {
            var productionId = await _productionService.AddAsync(productionCreationDto);
            return await Result<Guid>.SuccessAsync(productionId, "Production Added Successfully");
        }

        [Authorize(Permissions.Productions.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] ProductionUpdateDto productionUpdateDto)
        {
            var productionId = await _productionService.UpdateAsync(productionUpdateDto);
            return await Result<Guid>.SuccessAsync(productionId, "Production Updated Successfully");
        }

        [Authorize(Permissions.Productions.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var productionId = await _productionService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(productionId, "Production Deleted Successfully");
        }

        [Authorize(Permissions.Productions.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _productionService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ProductionStatus.Checked, "Production Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.Productions.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _productionService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ProductionStatus.Approved, "Production Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.Productions.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _productionService.UnpostAsync(id, fromStatus), "Production Unposted Successfully");
        }

        [Authorize(Permissions.ProductionModuleReports.Production_Details)]
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> PrintProductionReport(ProductionRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "ProductionNo";
            request.ProductionStatus = (int)ProductionStatus.Approved;
            var list = await _productionService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Production : Details (Without Value)";
                var tables = await _productionPdfService.PrintProductionReportToPdf(stream, list.Item1.ToList(), headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
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

        [Authorize(Permissions.ProductionModuleReports.Production_Details2)]
        [HttpPost]
        [Route("production-details-print")]
        public virtual async Task<IActionResult> PrintProductionDetailsReport(ProductionRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "ProductionNo";
            request.ProductionStatus = (int)ProductionStatus.Approved;
            var list = await _productionService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Production : Details (With Value)";
                var tables = await _productionPdfService.PrintProductionDetailsReportToPdf(stream, list.Item1.ToList(), headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
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
    }
}
