using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.PurchaseReturn;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.Services.Purchase.PurchaseReturns;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseReturn;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("purchase")]
    public class PurchaseReturnController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseReturnService _purchaseReturnService;
        private readonly IPurchasePdfService _purchasePdfService;

        public PurchaseReturnController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IPurchaseReturnService purchaseReturnService, IPurchasePdfService purchasePdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _purchaseReturnService = purchaseReturnService;
            _purchasePdfService = purchasePdfService;
        }

        [Authorize(Permissions.PurchaseReturns.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PurchaseReturnViewModel>, int>>> Search(PurchaseReturnRequestModel request)
        {
            return await Result<Tuple<List<PurchaseReturnViewModel>, int>>.SuccessAsync(await _purchaseReturnService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.PurchaseReturns.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<PurchaseReturnAggregatorModel>> ReportAggregates(PurchaseReturnRequestModel request)
        {
            return await Result<PurchaseReturnAggregatorModel>.SuccessAsync(await _purchaseReturnService.PreparePurchaseReturnAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.PurchaseReturns.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<PurchaseReturnViewModel>> GetById(Guid id)
        {
            return await Result<PurchaseReturnViewModel>.SuccessAsync(await _purchaseReturnService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.PurchaseReturns.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _purchaseReturnService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.PurchaseReturns.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PurchaseReturnCreationDto purchaseReturnCreationDto)
        {
            var purchaseReturnId = await _purchaseReturnService.AddAsync(purchaseReturnCreationDto);
            return await Result<Guid>.SuccessAsync(purchaseReturnId, "Purchase Return Added Successfully");
        }

        [Authorize(Permissions.PurchaseReturns.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] PurchaseReturnUpdateDto purchaseReturnUpdateDto)
        {
            var purchaseReturnId = await _purchaseReturnService.UpdateAsync(purchaseReturnUpdateDto);
            return await Result<Guid>.SuccessAsync(purchaseReturnId, "Purchase Return Updated Successfully");
        }

        [Authorize(Permissions.PurchaseReturns.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var purchaseReturnId = await _purchaseReturnService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(purchaseReturnId, "Purchase Return Deleted Successfully");
        }

        [Authorize(Permissions.PurchaseReturns.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _purchaseReturnService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PurchaseReturnStatus.Checked, "Purchase Return Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseReturns.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _purchaseReturnService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PurchaseReturnStatus.Approved, "Purchase Return Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseReturns.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _purchaseReturnService.UnpostAsync(id, fromStatus), "Purchase Return Unposted Successfully");
        }

        [HttpPost]
        [Route("purchase-return-details-print")]
        public virtual async Task<IActionResult> PrintPurchaseReturnDetailsReport(PurchaseReturnRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "PurchaseReturnNo";
            request.PurchaseReturnStatuses = new List<int> { (int)PurchaseReturnStatus.Approved };
            var list = await _purchaseReturnService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintPurchaseReturnReportToPdf(stream, list.Item1.ToList(), "Purchase Return Details", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("purchase-return-summary-print")]
        public virtual async Task<IActionResult> PrintPurchaseReturnSummaryReport(PurchaseReturnRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "PurchaseReturnNo";
            request.PurchaseReturnStatuses = new List<int> { (int)PurchaseReturnStatus.Approved };
            var list = await _purchaseReturnService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintPurchaseReturnReportToPdf(stream, list.Item1.ToList(), "Purchase Return Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
