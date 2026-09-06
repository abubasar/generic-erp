using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.CustomerWiseProductDiscount;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.CustomerWiseProductDiscounts;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerWiseProductDiscountController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerWiseProductDiscountService _customerWiseProductDiscountService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public CustomerWiseProductDiscountController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ICustomerWiseProductDiscountService customerWiseProductDiscountService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _customerWiseProductDiscountService = customerWiseProductDiscountService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<CustomerWiseProductDiscountViewModel>, int>>> Search(CustomerWiseProductDiscountRequestModel request)
        {
            return await Result<Tuple<List<CustomerWiseProductDiscountViewModel>, int>>.SuccessAsync(await _customerWiseProductDiscountService.SearchAsync(request), "Result Found");
        }
        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<CustomerWiseProductDiscountViewModel>> GetById(Guid id)
        {
            return await Result<CustomerWiseProductDiscountViewModel>.SuccessAsync(await _customerWiseProductDiscountService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _customerWiseProductDiscountService.GetPendingCheckedCountAsync(), "Success");
        }
        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] CustomerWiseProductDiscountCreationDto customerWiseProductDiscountCreationDto)
        {
            var customerWiseProductDiscountId = await _customerWiseProductDiscountService.AddAsync(customerWiseProductDiscountCreationDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(customerWiseProductDiscountId, "Customer Wise Product Discount Added Successfully");
        }
        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] CustomerWiseProductDiscountUpdateDto customerWiseProductDiscountUpdateDto)
        {
            var customerWiseProductDiscountId = await _customerWiseProductDiscountService.UpdateAsync(customerWiseProductDiscountUpdateDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(customerWiseProductDiscountId, "Customer Wise Product Discount Updated Successfully");
        }
        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var customerWiseProductDiscountId = await _customerWiseProductDiscountService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(customerWiseProductDiscountId, "Customer Wise Product Discount Deleted Successfully");
        }

        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _customerWiseProductDiscountService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)CustomerWiseProductDiscountStatus.Checked, "Customer Wise Product Discount Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _customerWiseProductDiscountService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)CustomerWiseProductDiscountStatus.Approved, "Customer Wise Product Discount Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _customerWiseProductDiscountService.UnpostAsync(id, fromStatus), "Customer Wise Product Discount Unposted Successfully");
        }

        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.View)]
        [Route("discount/{customerId}/{productId}")]
        [HttpGet]
        public virtual async Task<Result> GetCustomerDiscount(Guid customerId, Guid productId)
        {
            var discount = await _customerWiseProductDiscountService.GetCustomerDiscountByProductId(customerId, productId);
            return await Result<CustomerInvoiceDiscountViewModel?>.SuccessAsync(discount, "Customer Wise Product Discount Found Successfully");
        }

        [Authorize(SecondaryPermissions.CustomerWiseProductDiscounts.View)]
        [Route("active-discount/{customerId}")]
        [HttpGet]
        public virtual async Task<Result> GetActiveCustomerDiscountByCustomerId(Guid customerId)
        {
            var discount = await _customerWiseProductDiscountService.GetActiveCustomerDiscountByCustomerId(customerId);
            return await Result<List<ActiveCustomerWiseProductDiscountByCustomerIdViewModel>>.SuccessAsync(discount, "Active Customer Wise Product Discount Found Successfully");
        }
        [HttpPost]
        [Route("customer-wise-product-discount-print")]
        public virtual async Task<IActionResult> PrintCustomerWiseProductDiscountReport(CustomerWiseProductDiscountRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            request.Page = -1;
            request.IsAscending = true;
            var list = await _customerWiseProductDiscountService.SearchAsync(request);
            byte[] bytes;

            using (var stream = new MemoryStream())
            {
                await _configurationPdfService.PrintCustomerWiseProductDiscountReportToPdf(stream, list.Item1.ToList(), "Customer Wise Product Discount", request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
