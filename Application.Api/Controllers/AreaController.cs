using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Area;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Areas;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreaController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAreaService _areaService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public AreaController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IAreaService areaService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _areaService = areaService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Areas.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<AreaViewModel>, int>>> Search(AreaRequestModel request)
        {
            return await Result<Tuple<List<AreaViewModel>, int>>.SuccessAsync(await _areaService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Areas.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] AreaCreationDto areaCreationDto)
        {
            var areaId = await _areaService.AddAsync(areaCreationDto);
            return await Result<Guid>.SuccessAsync(areaId, "Area Added Successfully");
        }
        [Authorize(Permissions.Areas.Edit)]
        [HttpPost("/api/area/update")]
        public virtual async Task<Result> Put([FromBody] AreaUpdateDto areaUpdateDto)
        {
            var areaId = await _areaService.UpdateAsync(areaUpdateDto);
            return await Result<Guid>.SuccessAsync(areaId, "Area Updated Successfully");
        }
        [Authorize(Permissions.Areas.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var areaId = await _areaService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(areaId, "Area Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(AreaRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _areaService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Area", "Zone", "Region" };
                List<float> columnWidths = new List<float> { 10f, 30f, 30f, 30f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var area in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        Area = area?.Name,
                        Zone = area?.Zone?.Name,
                        Region = area?.Zone?.Region?.Name
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Area List");
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
