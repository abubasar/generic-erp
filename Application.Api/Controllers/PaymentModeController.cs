using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.PaymentMode;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.PaymentModes;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentModeController : ControllerBase
    {
        private readonly string cacheKey = "payment-modes";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentModeService _paymentModeService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public PaymentModeController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork,
            IPaymentModeService paymentModeService, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _paymentModeService = paymentModeService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }
        [Authorize(Permissions.PaymentModes.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PaymentModeViewModel>, int>>> Search(PaymentModeRequestModel request)
        {
            return await Result<Tuple<List<PaymentModeViewModel>, int>>.SuccessAsync(await _paymentModeService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.PaymentMethods.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<PaymentModeViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<PaymentModeViewModel>, int> cachedList))
            {
                cachedList = await _paymentModeService.SearchAsync(new PaymentModeRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<PaymentModeViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.PaymentModes.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PaymentModeCreationDto paymentModeCreationDto)
        {
            var paymentModeId = await _paymentModeService.AddAsync(paymentModeCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(paymentModeId, "Payment Mode Added Successfully");
        }
        [Authorize(Permissions.PaymentModes.Edit)]
        [HttpPost("/api/paymentMode/update")]
        public virtual async Task<Result> Put([FromBody] PaymentModeUpdateDto paymentModeUpdateDto)
        {
            var paymentModeId = await _paymentModeService.UpdateAsync(paymentModeUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(paymentModeId, "Payment Mode Updated Successfully");
        }
        [Authorize(Permissions.PaymentModes.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var paymentModeId = await _paymentModeService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(paymentModeId, "Payment Mode Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(PaymentModeRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _paymentModeService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var paymentMode in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        paymentMode?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Payment Mode List");
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
