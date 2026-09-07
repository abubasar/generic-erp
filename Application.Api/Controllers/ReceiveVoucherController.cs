using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.ReceiveVoucher;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Accounts.PdfAccount;
using Application.Services.Services.Accounts.ReceiveVouchers;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.ReceiveVoucher;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("accounts")]
    public class ReceiveVoucherController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReceiveVoucherService _receiveVoucherService;
        private readonly IAccountService _accountService;
        private readonly IAccountReportPdfService _accountReportPdfService;

        public ReceiveVoucherController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork,
            IReceiveVoucherService receiveVoucherService, IAccountService accountService, IAccountReportPdfService accountReportPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _receiveVoucherService = receiveVoucherService;
            _accountService = accountService;
            _accountReportPdfService = accountReportPdfService;
        }

        [Authorize(Permissions.ReceiveVouchers.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ReceiveVoucherViewModel>, int>>> Search(ReceiveVoucherRequestModel request)
        {
            return await Result<Tuple<List<ReceiveVoucherViewModel>, int>>.SuccessAsync(await _receiveVoucherService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.ReceiveVouchers.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<ReceiveVoucherAggregatorModel>> ReportAggregates(ReceiveVoucherRequestModel request)
        {
            return await Result<ReceiveVoucherAggregatorModel>.SuccessAsync(await _receiveVoucherService.PrepareReceiveVoucherAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.ReceiveVouchers.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<ReceiveVoucherViewModel>> GetById(Guid id)
        {
            return await Result<ReceiveVoucherViewModel>.SuccessAsync(await _receiveVoucherService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.ReceiveVouchers.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _receiveVoucherService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.ReceiveVouchers.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ReceiveVoucherCreationDto receiveVoucherCreationDto)
        {
            var receiveVoucherId = await _receiveVoucherService.AddAsync(receiveVoucherCreationDto);
            return await Result<Guid>.SuccessAsync(receiveVoucherId, "Receive Voucher Added Successfully");
        }

        [Authorize(Permissions.ReceiveVouchers.Edit)]
        [HttpPost("/api/receiveVoucher/update")]
        public virtual async Task<Result> Put([FromBody] ReceiveVoucherUpdateDto receiveVoucherUpdateDto)
        {
            var receiveVoucherId = await _receiveVoucherService.UpdateAsync(receiveVoucherUpdateDto);
            return await Result<Guid>.SuccessAsync(receiveVoucherId, "Receive Voucher Updated Successfully");
        }

        [Authorize(Permissions.ReceiveVouchers.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var receiveVoucherId = await _receiveVoucherService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(receiveVoucherId, "Receive Voucher Deleted Successfully");
        }

        [Authorize(Permissions.ReceiveVouchers.Check)]
        [HttpPost("/api/receiveVoucher/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _receiveVoucherService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ReceiveVoucherStatus.Checked, "Receive Voucher Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.ReceiveVouchers.Approve)]
        [HttpPost("/api/receiveVoucher/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _receiveVoucherService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ReceiveVoucherStatus.Approved, "Receive Voucher Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.ReceiveVouchers.Unpost)]
        [HttpPost("/api/receiveVoucher/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _receiveVoucherService.UnpostAsync(id, fromStatus), "Receive Voucher Unposted Successfully");
        }
    }
}
