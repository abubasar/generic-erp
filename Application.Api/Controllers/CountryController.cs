using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Country;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Countries;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly string cacheKey = "countries";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workerContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryService _countryService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;

        public CountryController(IMapper mapper, IWorkContext workerContext, IUnitOfWork unitOfWork, ICountryService countryService, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _mapper = mapper;
            _workerContext = workerContext;
            _unitOfWork = unitOfWork;
            _countryService = countryService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }

        [Authorize(Permissions.Countries.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<CountryViewModel>, int>>> Search(CountryRequestModel request)
        {
            return await Result<Tuple<List<CountryViewModel>, int>>.SuccessAsync(await _countryService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Countries.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<CountryViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<CountryViewModel>, int> cacheList))
            {
                cacheList = await _countryService.SearchAsync(new CountryRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cacheList);
            }
            return await Result<Tuple<List<CountryViewModel>, int>>.SuccessAsync(cacheList, "Result Found");
        }

        [Authorize(Permissions.Countries.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] CountryCreationDto countryCreationDto)
        {
            var countryId = await _countryService.AddAsync(countryCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(countryId, "Country Added Successfully");
        }

        [Authorize(Permissions.Countries.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] CountryUpdateDto countryUpdateDto)
        {
            var countryId = await _countryService.UpdateAsync(countryUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(countryId, "Country Updated Successfully");
        }

        [Authorize(Permissions.Countries.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var countryId = await _countryService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(countryId, "Country Deleted Successfullly");
        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(CountryRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _countryService.SearchAsync(request);
                //Headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                //Create a list of data with properties mapped of header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var country in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        country?.Name
                    });

                }

                byte[] bytes;
                using (var ms = new MemoryStream())
                {

                    await _configurationPdfService.PrintReportToPdfAsync(ms, headers, columnWidths, tableData, "Country List");
                    bytes = ms.ToArray();
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
