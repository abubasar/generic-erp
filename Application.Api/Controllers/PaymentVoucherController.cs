using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.PaymentVoucher;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Accounts.PaymentVouchers;
using Application.Services.Services.Accounts.PdfAccount;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.PaymentVoucher;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentVoucherController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentVoucherService _paymentVoucherService;
        private readonly IAccountService _accountService;
        private readonly IAccountReportPdfService _accountReportPdfService;

        public PaymentVoucherController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork,
            IPaymentVoucherService paymentVoucherService, IAccountService accountService, IAccountReportPdfService accountReportPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _paymentVoucherService = paymentVoucherService;
            _accountService = accountService;
            _accountReportPdfService = accountReportPdfService;
        }

        [Authorize(Permissions.PaymentVouchers.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PaymentVoucherViewModel>, int>>> Search(PaymentVoucherRequestModel request)
        {
            return await Result<Tuple<List<PaymentVoucherViewModel>, int>>.SuccessAsync(await _paymentVoucherService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.PaymentVouchers.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<PaymentVoucherAggregatorModel>> ReportAggregates(PaymentVoucherRequestModel request)
        {
            return await Result<PaymentVoucherAggregatorModel>.SuccessAsync(await _paymentVoucherService.PreparePaymentVoucherAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.PaymentVouchers.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<PaymentVoucherViewModel>> GetById(Guid id)
        {
            return await Result<PaymentVoucherViewModel>.SuccessAsync(await _paymentVoucherService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.PaymentVouchers.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _paymentVoucherService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.PaymentVouchers.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PaymentVoucherCreationDto paymentVoucherCreationDto)
        {
            var paymentVoucherId = await _paymentVoucherService.AddAsync(paymentVoucherCreationDto);
            return await Result<Guid>.SuccessAsync(paymentVoucherId, "Payment Voucher Added Successfully");
        }

        [Authorize(Permissions.PaymentVouchers.Edit)]
        [HttpPost("/api/paymentVoucher/update")]
        public virtual async Task<Result> Put([FromBody] PaymentVoucherUpdateDto paymentVoucherUpdateDto)
        {
            var paymentVoucherId = await _paymentVoucherService.UpdateAsync(paymentVoucherUpdateDto);
            return await Result<Guid>.SuccessAsync(paymentVoucherId, "Payment Voucher Updated Successfully");
        }

        [Authorize(Permissions.PaymentVouchers.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var paymentVoucherId = await _paymentVoucherService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(paymentVoucherId, "Payment Voucher Deleted Successfully");
        }

        [Authorize(Permissions.PaymentVouchers.Check)]
        [HttpPost("/api/paymentVoucher/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _paymentVoucherService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PaymentVoucherStatus.Checked, "Payment Voucher Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.PaymentVouchers.Approve)]
        [HttpPost("/api/paymentVoucher/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _paymentVoucherService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PaymentVoucherStatus.Approved, "Payment Voucher Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.PaymentVouchers.Unpost)]
        [HttpPost("/api/paymentVoucher/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _paymentVoucherService.UnpostAsync(id, fromStatus), "Payment Voucher Unposted Successfully");
        }
    }
}
