using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Designation;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Designations;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignationController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDesignationService _designationService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public DesignationController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IDesignationService designationService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _designationService = designationService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Designations.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<DesignationViewModel>, int>>> Search(DesignationRequestModel request)
        {
            return await Result<Tuple<List<DesignationViewModel>, int>>.SuccessAsync(await _designationService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Designations.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] DesignationCreationDto designationCreationDto)
        {
            var designationId = await _designationService.AddAsync(designationCreationDto);
            return await Result<Guid>.SuccessAsync(designationId, "Designation Added Successfully");
        }
        [Authorize(Permissions.Designations.Edit)]
        [HttpPost("/api/designation/update")]
        public virtual async Task<Result> Put([FromBody] DesignationUpdateDto designationUpdateDto)
        {
            var designationId = await _designationService.UpdateAsync(designationUpdateDto);
            return await Result<Guid>.SuccessAsync(designationId, "Designation Updated Successfully");
        }
        [Authorize(Permissions.Designations.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var designationId = await _designationService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(designationId, "Designation Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(DesignationRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _designationService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name", "Department" };
                List<float> columnWidths = new List<float> { 10f, 45f, 45f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var designation in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        designation?.Name,
                        Department = designation?.Department?.Name

                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Designation List");
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
