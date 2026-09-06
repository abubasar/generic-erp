using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.LcAdjustment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Purchase.LcAdjustments;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LcAdjustmentController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILcAdjustmentService _lcAdjustmentService;
        private readonly IPurchasePdfService _purchasePdfService;

        public LcAdjustmentController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ILcAdjustmentService lcAdjustmentService, IPurchasePdfService purchasePdfService
            )
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _lcAdjustmentService = lcAdjustmentService;
            _purchasePdfService = purchasePdfService;
        }

        [Authorize(Permissions.LcAdjustments.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<LcAdjustmentViewModel>, int>>> Search(LcAdjustmentRequestModel request)
        {
            return await Result<Tuple<List<LcAdjustmentViewModel>, int>>.SuccessAsync(await _lcAdjustmentService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.LcAdjustments.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<LcAdjustmentViewModel>> GetById(Guid id)
        {
            return await Result<LcAdjustmentViewModel>.SuccessAsync(await _lcAdjustmentService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.LcAdjustments.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _lcAdjustmentService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.LcAdjustments.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] LcAdjustmentCreationDto lcAdjustmentCreationDto)
        {
            var lcAdjustmentId = await _lcAdjustmentService.AddAsync(lcAdjustmentCreationDto);
            return await Result<Guid>.SuccessAsync(lcAdjustmentId, "LC Adjustment Added Successfully");
        }

        [Authorize(Permissions.LcAdjustments.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] LcAdjustmentUpdateDto lcAdjustmentUpdateDto)
        {
            var lcAdjustmentId = await _lcAdjustmentService.UpdateAsync(lcAdjustmentUpdateDto);
            return await Result<Guid>.SuccessAsync(lcAdjustmentId, "LC Adjustment Updated Successfully");
        }

        [Authorize(Permissions.LcAdjustments.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var lcAdjustmentId = await _lcAdjustmentService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(lcAdjustmentId, "LC Adjustment Deleted Successfully");
        }

        [Authorize(Permissions.LcAdjustments.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _lcAdjustmentService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)LcAdjustmentStatus.Checked, "LC Adjustment Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.LcAdjustments.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _lcAdjustmentService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)LcAdjustmentStatus.Approved, "LC Adjustment Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.LcAdjustments.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _lcAdjustmentService.UnpostAsync(id, fromStatus), "LC Adjustment Unposted Successfully");
        }

        [HttpPost]
        [Route("lc-adjustment-summary-print")]
        public virtual async Task<IActionResult> PrintLcAdjustmentSummaryReport(LcAdjustmentRequestModel request)
        {
            var reportTitle = "LC Adjustment Summary";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Code";
            request.LcAdjustmentStatus = (int)LcAdjustmentStatus.Approved;
            var list = await _lcAdjustmentService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintLcAdjustmentReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("lc-adjustment-details-print")]
        public virtual async Task<IActionResult> PrintLcAjustmentDetailsReport(LcAdjustmentRequestModel request)
        {
            var reportTitle = "LC Adjustment Details";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Code";
            request.LcAdjustmentStatus = (int)LcAdjustmentStatus.Approved;
            var list = await _lcAdjustmentService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintLcAdjustmentReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
