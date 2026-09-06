using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Services.Dtos.Configuration.Manufacturer;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Manufacturers;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturerController : ControllerBase
    {
        private readonly string cacheKey = "manufacturers";
        private readonly IManufacturerService _manufacturerService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public ManufacturerController(IManufacturerService manufacturerService, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _manufacturerService = manufacturerService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }

        [Authorize(Permissions.Manufacturers.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ManufacturerViewModel>, int>>> Search(ManufacturerRequestModel request)
        {
            return await Result<Tuple<List<ManufacturerViewModel>, int>>.SuccessAsync(await _manufacturerService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Manufacturers.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<ManufacturerViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<ManufacturerViewModel>, int> cachedList))
            {
                cachedList = await _manufacturerService.SearchAsync(new ManufacturerRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<ManufacturerViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }

        [Authorize(Permissions.Manufacturers.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ManufacturerCreationDto manufacturerCreationDto)
        {
            var manufacturerId = await _manufacturerService.AddAsync(manufacturerCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(manufacturerId, "Manufacturer Added Successfully");
        }

        [Authorize(Permissions.Manufacturers.Edit)]
        [HttpPost("/api/Manufacturer/update")]
        public virtual async Task<Result> Put([FromBody] ManufacturerUpdateDto manufacturerUpdateDto)
        {
            var manufacturerId = await _manufacturerService.UpdateAsync(manufacturerUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(manufacturerId, "Manufacturer Updated Successfully");
        }

        [Authorize(Permissions.Manufacturers.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var manufacturerId = await _manufacturerService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(manufacturerId, "Manufacturer Deleted Successfully");
        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(ManufacturerRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _manufacturerService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var manufacturer in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        manufacturer?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Manufacturer List");
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
