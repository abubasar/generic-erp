using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Customers;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Accounts;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly string cacheKey = "customers";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerService _customerService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public CustomerController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ICustomerService customerService,
            IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _customerService = customerService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }
        [Authorize(Permissions.Customers.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<CustomerViewModel>, int>>> Search(CustomerRequestModel request)
        {
            return await Result<Tuple<List<CustomerViewModel>, int>>.SuccessAsync(await _customerService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Customers.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<CustomerViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<CustomerViewModel>, int> cachedList))
            {
                cachedList = await _customerService.SearchAsync(new CustomerRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<CustomerViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.Customers.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] CustomerCreationDto customerCreationDto)
        {
            if (await _customerService.CustomerExists(null, customerCreationDto.Name ?? "", customerCreationDto.ContactNo ?? "")) return await Result<string>.FailAsync("", "A customer with the same name and contact number already exists.");
            var customerId = await _customerService.AddAsync(customerCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(customerId, "Customer Added Successfully");
        }
        [Authorize(Permissions.Customers.Edit)]
        [HttpPost("/api/customer/update")]
        public virtual async Task<Result> Put([FromBody] CustomerUpdateDto customerUpdateDto)
        {
            if (await _customerService.CustomerExists(customerUpdateDto.Id.ToString(), customerUpdateDto.Name ?? "", customerUpdateDto.ContactNo ?? "")) return await Result<string>.FailAsync("", "A customer with the same name and contact number already exists.");
            var customerId = await _customerService.UpdateAsync(customerUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(customerId, "Customer Updated Successfully");
        }
        [Authorize(Permissions.Customers.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var customerId = await _customerService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(customerId, "Customer Deleted Successfully");
        }
        [Route("get/customer-credit-limit-balance/{customerId}")]
        [HttpGet]
        public async Task<Result> GetCustomerCreditLimitAndBalance(Guid customerId)
        {
            var customerInfo = await _customerService.FindCustomerCreditLimitAndBalance(customerId);
            return await Result<object>.SuccessAsync(new { CreditLimit = customerInfo.creditLimit, Balance = customerInfo.balance }, "Result Found");
        }
        [Route("get/customer-balance/{customerId}")]
        [HttpGet]
        public async Task<Result> GetCustomerBalance(Guid customerId)
        {
            var balance = await _customerService.FindCustomerBalance(customerId);
            return await Result<object>.SuccessAsync(new { Balance = balance }, "Result Found");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(CustomerRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.OrderBy = "Name";
                request.IsAscending = true;
                var list = await _customerService.SearchAsync(request);
                var userName = _workContext.GetUserName() ?? "";
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintCustomerReportToPdfAsync(stream, list.Item1.ToList(), "Customer List", request);
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }
    }
}
