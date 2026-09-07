using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.VendorQuotation;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Purchase.VendorQuotations;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("purchase")]
    public class VendorQuotationController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVendorQuotationService _vendorQuotationService;
        public VendorQuotationController(IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IVendorQuotationService vendorQuotationService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _vendorQuotationService = vendorQuotationService;
        }
        [Authorize(Permissions.VendorQuotations.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<VendorQuotationViewModel>, int>>> Search(VendorQuotationRequestModel request)
        {
            return await Result<Tuple<List<VendorQuotationViewModel>, int>>.SuccessAsync(await _vendorQuotationService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.VendorQuotations.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<VendorQuotationViewModel>> GetById(Guid id)
        {
            return await Result<VendorQuotationViewModel>.SuccessAsync(await _vendorQuotationService.GetByIdAsync(id), "Result Found");
        }
        [Authorize(Permissions.VendorQuotations.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] VendorQuotationCreationDto vendorQuotationCreationDto)
        {
            var addUpdateResponseModel = await _vendorQuotationService.AddAsync(vendorQuotationCreationDto);
            return await Result<Guid>.SuccessAsync(addUpdateResponseModel, "Vendor Quotation Added Successfully");
        }
        [Authorize(Permissions.VendorQuotations.Edit)]
        [HttpPost("/api/vendorQuotation/update")]
        public virtual async Task<Result> Put([FromBody] VendorQuotationUpdateDto vendorQuotationUpdateDto)
        {
            var addUpdateResponseModel = await _vendorQuotationService.UpdateAsync(vendorQuotationUpdateDto);
            return await Result<Guid>.SuccessAsync(addUpdateResponseModel, "VendorQuotation Updated Successfully");
        }
        [Authorize(Permissions.VendorQuotations.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var vendorQuotationId = await _vendorQuotationService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(vendorQuotationId, "VendorQuotation Deleted Successfully");
        }
        [Authorize(Permissions.VendorQuotations.ApproveVendorQuotation)]
        [HttpPost("/api/vendorQuotation/approve/{id}/{requisitionNo}")]
        public virtual async Task<Result> ApproveVendorQuotation(Guid id, string requisitionNo)
        {
            var isApproved = await _vendorQuotationService.ApproveQuotationAsync(id, requisitionNo);
            if (isApproved) return await Result<int>.SuccessAsync((int)VendorQuotationStatus.Approved, "Vendor Quotation Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }
        [Authorize(Permissions.VendorQuotations.Unpost)]
        [HttpPost("/api/vendorQuotation/unpost/{requisitionNo}")]
        public virtual async Task<Result> CancelVendorQuotation(string requisitionNo)
        {
            var success = await _vendorQuotationService.UnpostQuotationAsync(requisitionNo);
            if (success) return await Result<string>.SuccessAsync("Canceled", "Vendor Quotation Unposted Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }
    }
}
