using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Currency;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Currencies;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyService _currencyService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public CurrencyController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ICurrencyService currencyService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _currencyService = currencyService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Currencies.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<CurrencyViewModel>, int>>> Search(CurrencyRequestModel request)
        {
            return await Result<Tuple<List<CurrencyViewModel>, int>>.SuccessAsync(await _currencyService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Currencies.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] CurrencyCreationDto currencyCreationDto)
        {
            var currencyId = await _currencyService.AddAsync(currencyCreationDto);
            return await Result<Guid>.SuccessAsync(currencyId, "Currency Added Successfully");
        }
        [Authorize(Permissions.Currencies.Edit)]
        [HttpPost("/api/currency/update")]
        public virtual async Task<Result> Put([FromBody] CurrencyUpdateDto currencyUpdateDto)
        {
            var currencyId = await _currencyService.UpdateAsync(currencyUpdateDto);
            return await Result<Guid>.SuccessAsync(currencyId, "Currency Updated Successfully");
        }
        [Authorize(Permissions.Currencies.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var currencyId = await _currencyService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(currencyId, "Currency Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(CurrencyRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _currencyService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var currency in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        currency?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Currency List");
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
