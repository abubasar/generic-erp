
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.MeasurementUnit;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.MeasurementUnits;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeasurementUnitController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMeasurementUnitService _measurementUnitService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public MeasurementUnitController(IMeasurementUnitService measurementUnitService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _measurementUnitService = measurementUnitService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.MeasurementUnits.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<MeasurementUnitViewModel>, int>>> Search(MeasurementUnitRequestModel request)
        {
            return await Result<Tuple<List<MeasurementUnitViewModel>, int>>.SuccessAsync(await _measurementUnitService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.MeasurementUnits.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] MeasurementUnitCreationDto measurementUnitCreationDto)
        {
            var measurementUnitId = await _measurementUnitService.AddAsync(measurementUnitCreationDto);
            return await Result<Guid>.SuccessAsync(measurementUnitId, "MeasurementUnit Added Successfully");
        }
        [Authorize(Permissions.MeasurementUnits.Edit)]
        [HttpPost("/api/measurementUnit/update")]
        public virtual async Task<Result> Put([FromBody] MeasurementUnitUpdateDto measurementUnitUpdateDto)
        {
            var measurementUnitId = await _measurementUnitService.UpdateAsync(measurementUnitUpdateDto);
            return await Result<Guid>.SuccessAsync(measurementUnitId, "MeasurementUnit Updated Successfully");
        }
        [Authorize(Permissions.MeasurementUnits.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var measurementUnitId = await _measurementUnitService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(measurementUnitId, "MeasurementUnit Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(MeasurementUnitRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _measurementUnitService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var measurementUnit in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        measurementUnit?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Measurement Unit List");
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
