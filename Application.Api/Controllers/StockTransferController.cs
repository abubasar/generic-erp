using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Inventory.StockTransfer;
using Application.Services.SearchRequestModels.Inventory;
using Application.Services.Services.Common;
using Application.Services.Services.Inventory.StockTransfers;
using Application.Services.Services.Sale.Pdf;
using Application.Services.ViewModels.Inventory;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("inventory")]
    public class StockTransferController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockTransferService _stockTransferService;
        private readonly ISalePdfService _salePdfService;

        public StockTransferController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IStockTransferService stockTransferService, ISalePdfService salePdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _stockTransferService = stockTransferService;
            _salePdfService = salePdfService;
        }

        [Authorize(Permissions.StockTransfers.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<StockTransferViewModel>, int>>> Search(StockTransferRequestModel request)
        {
            return await Result<Tuple<List<StockTransferViewModel>, int>>.SuccessAsync(await _stockTransferService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.StockTransfers.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<StockTransferViewModel>> GetById(Guid id)
        {
            return await Result<StockTransferViewModel>.SuccessAsync(await _stockTransferService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.StockTransfers.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _stockTransferService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.StockTransfers.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] StockTransferCreationDto stockTransferCreationDto)
        {
            var stockTransferId = await _stockTransferService.AddAsync(stockTransferCreationDto);
            return await Result<Guid>.SuccessAsync(stockTransferId, "Stock Transfer Added Successfully");
        }

        [Authorize(Permissions.StockTransfers.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] StockTransferUpdateDto stockTransferUpdateDto)
        {
            var stockTransferId = await _stockTransferService.UpdateAsync(stockTransferUpdateDto);
            return await Result<Guid>.SuccessAsync(stockTransferId, "Stock Transfer Updated Successfully");
        }

        [Authorize(Permissions.StockTransfers.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var stockTransferId = await _stockTransferService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(stockTransferId, "Stock Transfer Deleted Successfully");
        }

        [Authorize(Permissions.StockTransfers.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _stockTransferService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)StockTransferStatus.Checked, "Stock Transfer Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.StockTransfers.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _stockTransferService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)StockTransferStatus.Approved, "Stock Transfer Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.StockTransfers.Unpost)]

        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _stockTransferService.UnpostAsync(id, fromStatus), "Stock Transfer Unposted Successfully");
        }

        [HttpPost]
        [Route("stock-transfer-summary-print")]
        public virtual async Task<IActionResult> PrintStockTransferSummaryReport(StockTransferRequestModel request)
        {
            var reportTitle = "Stock Transfer Summary";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "TransferNo";
            request.StockTransferStatus = (int)StockTransferStatus.Approved;
            var list = await _stockTransferService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintStockTransferReportToPdfAsync(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("stock-transfer-details-print")]
        public virtual async Task<IActionResult> PrintStockTransferDetailsReport(StockTransferRequestModel request)
        {
            var reportTitle = "Stock Transfer Details";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "TransferNo";
            request.StockTransferStatus = (int)StockTransferStatus.Approved;
            var list = await _stockTransferService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintStockTransferReportToPdfAsync(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
