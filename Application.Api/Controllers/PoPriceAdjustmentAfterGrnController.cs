using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.PoPriceAdjustmentAfterGrn;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.Services.Purchase.PoPriceAdjustmentAfterGrns;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PoPriceAdjustmentAfterGrn;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PoPriceAdjustmentAfterGrnController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPoPriceAdjustmentAfterGrnService _poPriceAdjustmentAfterGrnService;
        private readonly IPurchasePdfService _purchasePdfService;

        public PoPriceAdjustmentAfterGrnController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IPoPriceAdjustmentAfterGrnService poPriceAdjustmentAfterGrnService, IPurchasePdfService purchasePdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _poPriceAdjustmentAfterGrnService = poPriceAdjustmentAfterGrnService;
            _purchasePdfService = purchasePdfService;
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PoPriceAdjustmentAfterGrnViewModel>, int>>> Search(PoPriceAdjustmentAfterGrnRequestModel request)
        {
            return await Result<Tuple<List<PoPriceAdjustmentAfterGrnViewModel>, int>>.SuccessAsync(await _poPriceAdjustmentAfterGrnService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<PoPriceAdjustmentAfterGrnAggregatorModel>> ReportAggregates(PoPriceAdjustmentAfterGrnRequestModel request)
        {
            return await Result<PoPriceAdjustmentAfterGrnAggregatorModel>.SuccessAsync(await _poPriceAdjustmentAfterGrnService.PreparePoPriceAdjustmentAfterGrnAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<PoPriceAdjustmentAfterGrnViewModel>> GetById(Guid id)
        {
            return await Result<PoPriceAdjustmentAfterGrnViewModel>.SuccessAsync(await _poPriceAdjustmentAfterGrnService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _poPriceAdjustmentAfterGrnService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PoPriceAdjustmentAfterGrnCreationDto poPriceAdjustmentAfterGrnCreationDto)
        {
            var poPriceAdjustmentAfterGrnId = await _poPriceAdjustmentAfterGrnService.AddAsync(poPriceAdjustmentAfterGrnCreationDto);
            return await Result<Guid>.SuccessAsync(poPriceAdjustmentAfterGrnId, "Po Price Adjustment After Grn Added Successfully");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.Edit)]
        [HttpPost("/api/poPriceAdjustmentAfterGrn/update")]
        public virtual async Task<Result> Put([FromBody] PoPriceAdjustmentAfterGrnUpdateDto poPriceAdjustmentAfterGrnUpdateDto)
        {
            var poPriceAdjustmentAfterGrnId = await _poPriceAdjustmentAfterGrnService.UpdateAsync(poPriceAdjustmentAfterGrnUpdateDto);
            return await Result<Guid>.SuccessAsync(poPriceAdjustmentAfterGrnId, "Po Price Adjustment After Grn Updated Successfully");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var poPriceAdjustmentAfterGrnId = await _poPriceAdjustmentAfterGrnService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(poPriceAdjustmentAfterGrnId, "Po Price Adjustment After Grn Deleted Successfully");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.Check)]
        [HttpPost("/api/poPriceAdjustmentAfterGrn/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _poPriceAdjustmentAfterGrnService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PoPriceAdjustmentAfterGrnStatus.Checked, "Po Price Adjustment After Grn Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.Approve)]
        [HttpPost("/api/poPriceAdjustmentAfterGrn/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _poPriceAdjustmentAfterGrnService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PoPriceAdjustmentAfterGrnStatus.Approved, "Po Price Adjustment After Grn Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.PoPriceAdjustmentAfterGrns.Unpost)]
        [HttpPost("/api/poPriceAdjustmentAfterGrn/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _poPriceAdjustmentAfterGrnService.UnpostAsync(id, fromStatus), "Po Price Adjustment After Grn Unposted Successfully");
        }

        [HttpPost]
        [Route("po-price-adjustment-after-grn-details-print")]
        public virtual async Task<IActionResult> PrintPoPriceAdjustmentAfterGrnDetailsReport(PoPriceAdjustmentAfterGrnRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "PoPriceAdjustmentAfterGrnNo";
            request.PoPriceAdjustmentAfterGrnStatuses = new List<int> { (int)PoPriceAdjustmentAfterGrnStatus.Approved };
            var list = await _poPriceAdjustmentAfterGrnService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                // _purchasePdfService.PrintPoPriceAdjustmentAfterGrnReportToPdf(stream, list.Item1.ToList(), "Purchase Return Details", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("po-price-adjustment-after-grn-summary-print")]
        public virtual async Task<IActionResult> PrintPoPriceAdjustmentAfterGrnSummaryReport(PoPriceAdjustmentAfterGrnRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "PoPriceAdjustmentAfterGrnNo";
            request.PoPriceAdjustmentAfterGrnStatuses = new List<int> { (int)PoPriceAdjustmentAfterGrnStatus.Approved };
            var list = await _poPriceAdjustmentAfterGrnService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                //   _purchasePdfService.PrintPoPriceAdjustmentAfterGrnReportToPdf(stream, list.Item1.ToList(), "Purchase Return Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
