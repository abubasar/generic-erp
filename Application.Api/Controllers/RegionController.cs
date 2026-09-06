using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Region;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.Regions;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRegionService _regionService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public RegionController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IRegionService regionService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _regionService = regionService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Regions.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<RegionViewModel>, int>>> Search(RegionRequestModel request)
        {
            return await Result<Tuple<List<RegionViewModel>, int>>.SuccessAsync(await _regionService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Regions.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] RegionCreationDto regionCreationDto)
        {
            var regionId = await _regionService.AddAsync(regionCreationDto);
            return await Result<Guid>.SuccessAsync(regionId, "Region Added Successfully");
        }
        [Authorize(Permissions.Regions.Edit)]
        [HttpPost("/api/region/update")]
        public virtual async Task<Result> Put([FromBody] RegionUpdateDto regionUpdateDto)
        {
            var regionId = await _regionService.UpdateAsync(regionUpdateDto);
            return await Result<Guid>.SuccessAsync(regionId, "Region Updated Successfully");
        }
        [Authorize(Permissions.Regions.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var regionId = await _regionService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(regionId, "Region Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(RegionRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _regionService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var region in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        region?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Region List");
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
