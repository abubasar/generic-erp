using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.JobLocation;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.JobLocations;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobLocationController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJobLocationService _jobLocationService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public JobLocationController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IJobLocationService jobLocationService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _jobLocationService = jobLocationService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.JobLocations.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<JobLocationViewModel>, int>>> Search(JobLocationRequestModel request)
        {
            return await Result<Tuple<List<JobLocationViewModel>, int>>.SuccessAsync(await _jobLocationService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.JobLocations.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] JobLocationCreationDto jobLocationCreationDto)
        {
            var jobLocationId = await _jobLocationService.AddAsync(jobLocationCreationDto);
            return await Result<Guid>.SuccessAsync(jobLocationId, "Job Location Added Successfully");
        }
        [Authorize(Permissions.JobLocations.Edit)]
        [HttpPost("/api/jobLocation/update")]
        public virtual async Task<Result> Put([FromBody] JobLocationUpdateDto jobLocationUpdateDto)
        {
            var jobLocationId = await _jobLocationService.UpdateAsync(jobLocationUpdateDto);
            return await Result<Guid>.SuccessAsync(jobLocationId, "Job Location Updated Successfully");
        }
        [Authorize(Permissions.JobLocations.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var jobLocationId = await _jobLocationService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(jobLocationId, "Job Location Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(JobLocationRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _jobLocationService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var jobLocation in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        jobLocation?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Job Location List");
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
