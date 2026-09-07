using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Sale.SaleReturn;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.Services.Sale.Pdf;
using Application.Services.Services.Sale.SaleReturns;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleReturn;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("sales")]
    public class SaleReturnController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISaleReturnService _saleReturnService;
        private readonly ISalePdfService _salePdfService;

        public SaleReturnController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ISaleReturnService saleReturnService, ISalePdfService salePdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _saleReturnService = saleReturnService;
            _salePdfService = salePdfService;
        }

        [Authorize(Permissions.SaleReturns.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<SaleReturnViewModel>, int>>> Search(SaleReturnRequestModel request)
        {
            return await Result<Tuple<List<SaleReturnViewModel>, int>>.SuccessAsync(await _saleReturnService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.SaleReturns.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<SaleReturnAggregatorModel>> ReportAggregates(SaleReturnRequestModel request)
        {
            return await Result<SaleReturnAggregatorModel>.SuccessAsync(await _saleReturnService.PrepareSaleReturnAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.SaleReturns.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<SaleReturnViewModel>> GetById(Guid id)
        {
            return await Result<SaleReturnViewModel>.SuccessAsync(await _saleReturnService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.SaleReturns.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _saleReturnService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.SaleReturns.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] SaleReturnCreationDto saleReturnCreationDto)
        {
            var saleReturnId = await _saleReturnService.AddAsync(saleReturnCreationDto);
            return await Result<Guid>.SuccessAsync(saleReturnId, "Sale Return Added Successfully");
        }

        [Authorize(Permissions.SaleReturns.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] SaleReturnUpdateDto saleReturnUpdateDto)
        {
            var saleReturnId = await _saleReturnService.UpdateAsync(saleReturnUpdateDto);
            return await Result<Guid>.SuccessAsync(saleReturnId, "Sale Return Updated Successfully");
        }

        [Authorize(Permissions.SaleReturns.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var saleReturnId = await _saleReturnService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(saleReturnId, "Sale Return Deleted Successfully");
        }

        [Authorize(Permissions.SaleReturns.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _saleReturnService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SaleReturnStatus.Checked, "Sale Return Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleReturns.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _saleReturnService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SaleReturnStatus.Approved, "Sale Return Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleReturns.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _saleReturnService.UnpostAsync(id, fromStatus), "Sale Return Unposted Successfully");
        }

        [HttpPost]
        [Route("sale-return-summary-print")]
        public virtual async Task<IActionResult> PrintSaleReturnSummaryReport(SaleReturnRequestModel request)
        {
            var reportTitle = "Sale Return Summary";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "SaleReturnNo";
            request.SaleReturnStatus = (int)SaleReturnStatus.Approved;
            var list = await _saleReturnService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintSaleReturnReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("sale-return-details-print")]
        public virtual async Task<IActionResult> PrintSaleReturnDetailsReport(SaleReturnRequestModel request)
        {
            var reportTitle = "Sale Return Details";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "SaleReturnNo";
            request.SaleReturnStatus = (int)SaleReturnStatus.Approved;
            var list = await _saleReturnService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintSaleReturnReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
