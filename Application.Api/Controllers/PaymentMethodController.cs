using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.PaymentMethod;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.PaymentMethods;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController : ControllerBase
    {
        private readonly string cacheKey = "payment-methods";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public PaymentMethodController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork,
            IPaymentMethodService paymentMethodService, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _paymentMethodService = paymentMethodService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }
        [Authorize(Permissions.PaymentMethods.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PaymentMethodViewModel>, int>>> Search(PaymentMethodRequestModel request)
        {
            return await Result<Tuple<List<PaymentMethodViewModel>, int>>.SuccessAsync(await _paymentMethodService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.PaymentMethods.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<PaymentMethodViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<PaymentMethodViewModel>, int> cachedList))
            {
                cachedList = await _paymentMethodService.SearchAsync(new PaymentMethodRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<PaymentMethodViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.PaymentMethods.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PaymentMethodCreationDto paymentMethodCreationDto)
        {
            var paymentMethodId = await _paymentMethodService.AddAsync(paymentMethodCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(paymentMethodId, "Payment Method Added Successfully");
        }
        [Authorize(Permissions.PaymentMethods.Edit)]
        [HttpPost("/api/paymentMethod/update")]
        public virtual async Task<Result> Put([FromBody] PaymentMethodUpdateDto paymentMethodUpdateDto)
        {
            var paymentMethodId = await _paymentMethodService.UpdateAsync(paymentMethodUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(paymentMethodId, "Payment Method Updated Successfully");
        }
        [Authorize(Permissions.PaymentMethods.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var paymentMethodId = await _paymentMethodService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(paymentMethodId, "Payment Method Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(PaymentMethodRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _paymentMethodService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var paymentMethod in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        paymentMethod?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Payment Method List");
                    bytes = stream.ToArray();
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
