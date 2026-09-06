using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.StockAdjustment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Inventory.Pdf;
using Application.Services.Services.Inventory.StockAdjustments;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockAdjustmentController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockAdjustmentService _stockAdjustmentService;
        private readonly IInventoryPdfService _inventoryPdfService;

        public StockAdjustmentController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IStockAdjustmentService stockAdjustmentService, IInventoryPdfService inventoryPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _stockAdjustmentService = stockAdjustmentService;
            _inventoryPdfService = inventoryPdfService;
        }

        [Authorize(Permissions.StockAdjustments.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<StockAdjustmentViewModel>, int>>> Search(StockAdjustmentRequestModel request)
        {
            return await Result<Tuple<List<StockAdjustmentViewModel>, int>>.SuccessAsync(await _stockAdjustmentService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.StockAdjustments.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<StockAdjustmentViewModel>> GetById(Guid id)
        {
            return await Result<StockAdjustmentViewModel>.SuccessAsync(await _stockAdjustmentService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.StockAdjustments.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _stockAdjustmentService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.StockAdjustments.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] StockAdjustmentCreationDto stockAdjustmentCreationDto)
        {
            var stockAdjustmentId = await _stockAdjustmentService.AddAsync(stockAdjustmentCreationDto);
            return await Result<Guid>.SuccessAsync(stockAdjustmentId, "Stock Adjustment Added Successfully");
        }

        [Authorize(Permissions.StockAdjustments.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] StockAdjustmentUpdateDto stockAdjustmentUpdateDto)
        {
            var stockAdjustmentId = await _stockAdjustmentService.UpdateAsync(stockAdjustmentUpdateDto);
            return await Result<Guid>.SuccessAsync(stockAdjustmentId, "Stock Adjustment Updated Successfully");
        }

        [Authorize(Permissions.StockAdjustments.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var stockAdjustmentId = await _stockAdjustmentService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(stockAdjustmentId, "Stock Adjustment Deleted Successfully");
        }

        [Authorize(Permissions.StockAdjustments.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _stockAdjustmentService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)StockAdjustmentStatus.Checked, "Stock Adjustment Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.StockAdjustments.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _stockAdjustmentService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)StockAdjustmentStatus.Approved, "Stock Adjustment Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.StockAdjustments.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _stockAdjustmentService.UnpostAsync(id, fromStatus), "Stock Adjustment Unposted Successfully");
        }

        [HttpPost]
        [Route("stock-adjustment-summary-print")]
        public virtual async Task<IActionResult> PrintStockAdjustmentSummaryReport(StockAdjustmentRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Code";
            request.StockAdjustmentStatus = (int)StockAdjustmentStatus.Approved;
            var list = await _stockAdjustmentService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _inventoryPdfService.PrintStockAdjustmentReportToPdfAsync(stream, list.Item1.ToList(), "Stock Adjustment Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("stock-adjustment-details-print")]
        public virtual async Task<IActionResult> PrintStockAdjustmentDetailsReport(StockAdjustmentRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Code";
            request.StockAdjustmentStatus = (int)StockAdjustmentStatus.Approved;
            var list = await _stockAdjustmentService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _inventoryPdfService.PrintStockAdjustmentReportToPdfAsync(stream, list.Item1.ToList(), "Stock Adjustment Details", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
