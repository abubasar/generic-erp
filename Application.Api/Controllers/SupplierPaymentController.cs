using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.SupplierPayment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Purchase.SupplierPayments;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.SupplierPayment;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierPaymentController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISupplierPaymentService _supplierPaymentService;

        public SupplierPaymentController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ISupplierPaymentService supplierPaymentService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _supplierPaymentService = supplierPaymentService;
        }

        [Authorize(Permissions.SupplierPayments.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<SupplierPaymentViewModel>, int>>> Search(SupplierPaymentRequestModel request)
        {
            return await Result<Tuple<List<SupplierPaymentViewModel>, int>>.SuccessAsync(await _supplierPaymentService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.SupplierPayments.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<SupplierPaymentAggregatorModel>> ReportAggregates(SupplierPaymentRequestModel request)
        {
            return await Result<SupplierPaymentAggregatorModel>.SuccessAsync(await _supplierPaymentService.PrepareSupplierPaymentAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.SupplierPayments.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<SupplierPaymentViewModel>> GetById(Guid id)
        {
            return await Result<SupplierPaymentViewModel>.SuccessAsync(await _supplierPaymentService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.SupplierPayments.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _supplierPaymentService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.SupplierPayments.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] SupplierPaymentCreationDto supplierPaymentCreationDto)
        {
            var supplierPaymentId = await _supplierPaymentService.AddAsync(supplierPaymentCreationDto);
            return await Result<Guid>.SuccessAsync(supplierPaymentId, "Supplier Payment Added Successfully");
        }

        [Authorize(Permissions.SupplierPayments.Edit)]
        [HttpPost("/api/supplierPayment/update")]
        public virtual async Task<Result> Put([FromBody] SupplierPaymentUpdateDto supplierPaymentUpdateDto)
        {
            var supplierPaymentId = await _supplierPaymentService.UpdateAsync(supplierPaymentUpdateDto);
            return await Result<Guid>.SuccessAsync(supplierPaymentId, "Supplier Payment Updated Successfully");
        }

        [Authorize(Permissions.SupplierPayments.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var supplierPaymentId = await _supplierPaymentService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(supplierPaymentId, "Supplier Payment Deleted Successfully");
        }

        [Authorize(Permissions.SupplierPayments.Check)]
        [HttpPost("/api/supplierPayment/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _supplierPaymentService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SupplierPaymentStatus.Checked, "Supplier Payment Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.SupplierPayments.Approve)]
        [HttpPost("/api/supplierPayment/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _supplierPaymentService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SupplierPaymentStatus.Approved, "Supplier Payment Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.SupplierPayments.Unpost)]
        [HttpPost("/api/supplierPayment/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _supplierPaymentService.UnpostAsync(id, fromStatus), "Supplier Payment Unposted Successfully");
        }
    }
}
