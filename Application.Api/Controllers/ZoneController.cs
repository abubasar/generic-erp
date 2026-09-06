using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Zone;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.Zones;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoneController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IZoneService _zoneService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public ZoneController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IZoneService zoneService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _zoneService = zoneService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Zones.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ZoneViewModel>, int>>> Search(ZoneRequestModel request)
        {
            return await Result<Tuple<List<ZoneViewModel>, int>>.SuccessAsync(await _zoneService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Zones.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ZoneCreationDto zoneCreationDto)
        {
            var zoneId = await _zoneService.AddAsync(zoneCreationDto);
            return await Result<Guid>.SuccessAsync(zoneId, "Zone Added Successfully");
        }
        [Authorize(Permissions.Zones.Edit)]
        [HttpPost("/api/zone/update")]
        public virtual async Task<Result> Put([FromBody] ZoneUpdateDto zoneUpdateDto)
        {
            var zoneId = await _zoneService.UpdateAsync(zoneUpdateDto);
            return await Result<Guid>.SuccessAsync(zoneId, "Zone Updated Successfully");
        }
        [Authorize(Permissions.Zones.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var zoneId = await _zoneService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(zoneId, "Zone Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(ZoneRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _zoneService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name", "Region" };
                List<float> columnWidths = new List<float> { 10f, 45f, 45f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var zone in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        zone?.Name,
                        Region = zone?.Region?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Zone List");
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
