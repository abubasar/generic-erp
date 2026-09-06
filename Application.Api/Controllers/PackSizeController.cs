using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Services.Dtos.Configuration.PackSize;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.PackSizes;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackSizeController : ControllerBase
    {
        private readonly string cacheKey = "packSizes";
        private readonly IPackSizeService _packSizeService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;

        public PackSizeController(IPackSizeService packSizeService, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _packSizeService = packSizeService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }

        [Authorize(PrimaryPermissions.PackSizes.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PackSizeViewModel>, int>>> Search(PackSizeRequestModel request)
        {
            return await Result<Tuple<List<PackSizeViewModel>, int>>.SuccessAsync(await _packSizeService.SearchAsync(request), "Result Found");
        }

        [Authorize(PrimaryPermissions.PackSizes.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<PackSizeViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<PackSizeViewModel>, int> cacheList))
            {
                cacheList = await _packSizeService.SearchAsync(new PackSizeRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cacheList);
            }
            return await Result<Tuple<List<PackSizeViewModel>, int>>.SuccessAsync(cacheList, "Result Found");
        }

        [Authorize(PrimaryPermissions.PackSizes.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PackSizeCreationDto packSizeCreationDto)
        {
            var packSizeId = await _packSizeService.AddAsync(packSizeCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(packSizeId, "Pack Size Added Successfully");
        }

        [Authorize(PrimaryPermissions.PackSizes.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] PackSizeUpdateDto packSizeUpdateDto)
        {
            var packSizeId = await _packSizeService.UpdateAsync(packSizeUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(packSizeId, "Pack Size Updated Successfully");
        }

        [Authorize(PrimaryPermissions.PackSizes.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var packSizeId = await _packSizeService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(packSizeId, "Pack Size Deleted Successfully");
        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(PackSizeRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _packSizeService.SearchAsync(request);
                //Headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                //Create a list of data with properties mapped of header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var packSize in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        packSize?.Name
                    });

                }

                byte[] bytes;
                using (var ms = new MemoryStream())
                {

                    await _configurationPdfService.PrintReportToPdfAsync(ms, headers, columnWidths, tableData, "Pack Size List");
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
