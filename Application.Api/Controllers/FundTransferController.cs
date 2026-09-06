using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.FundTransfer;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.FundTransfers;
using Application.Services.Services.Accounts.Pdf;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.FundTransfer;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FundTransferController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFundTransferService _fundTransferService;
        private readonly IAccountPdfService _accountPdfService;

        public FundTransferController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IFundTransferService fundTransferService, IAccountPdfService accountPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _fundTransferService = fundTransferService;
            _accountPdfService = accountPdfService;
        }

        [Authorize(Permissions.FundTransfers.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<FundTransferViewModel>, int>>> Search(FundTransferRequestModel request)
        {
            return await Result<Tuple<List<FundTransferViewModel>, int>>.SuccessAsync(await _fundTransferService.SearchAsync(request), "Result Found");
        }


        [Authorize(Permissions.FundTransfers.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<FundTransferAggregatorModel>> ReportAggregates(FundTransferRequestModel request)
        {
            return await Result<FundTransferAggregatorModel>.SuccessAsync(await _fundTransferService.PrepareFundTransferAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.FundTransfers.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<FundTransferViewModel>> GetById(Guid id)
        {
            return await Result<FundTransferViewModel>.SuccessAsync(await _fundTransferService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.FundTransfers.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _fundTransferService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.FundTransfers.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] FundTransferCreationDto fundTransferCreationDto)
        {
            var fundTransferId = await _fundTransferService.AddAsync(fundTransferCreationDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(fundTransferId, "Fund Transfer Added Successfully");
        }

        [Authorize(Permissions.FundTransfers.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] FundTransferUpdateDto fundTransferUpdateDto)
        {
            var fundTransferId = await _fundTransferService.UpdateAsync(fundTransferUpdateDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(fundTransferId, "Fund Transfer Updated Successfully");
        }

        [Authorize(Permissions.FundTransfers.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var fundTransferId = await _fundTransferService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(fundTransferId, "Fund Transfer Deleted Successfully");
        }

        [Authorize(Permissions.FundTransfers.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _fundTransferService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)FundTransferStatus.Checked, "Fund Transfer Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.FundTransfers.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _fundTransferService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)FundTransferStatus.Approved, "Fund Transfer Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.FundTransfers.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _fundTransferService.UnpostAsync(id, fromStatus), "Fund Transfer Unposted Successfully");
        }

        [HttpPost]
        [Route("fund-transfer-summary-print")]
        public virtual async Task<IActionResult> FundTransferSummaryReport(FundTransferRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "fundTransferNo";
            request.FundTransferStatus = (int)FundTransferStatus.Approved;
            var list = await _fundTransferService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _accountPdfService.PrintFundTransferReportToPdf(stream, list.Item1.ToList(), "Fund Transfer Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
