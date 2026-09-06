
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Department;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Departments;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDepartmentService _departmentService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public DepartmentController(IDepartmentService departmentService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _departmentService = departmentService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Departments.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<DepartmentViewModel>, int>>> Search(DepartmentRequestModel request)
        {
            return await Result<Tuple<List<DepartmentViewModel>, int>>.SuccessAsync(await _departmentService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Departments.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] DepartmentCreationDto departmentCreationDto)
        {
            var departmentId = await _departmentService.AddAsync(departmentCreationDto);
            return await Result<Guid>.SuccessAsync(departmentId, "Department Added Successfully");

        }
        [Authorize(Permissions.Departments.Edit)]
        [HttpPost("/api/department/update")]
        public virtual async Task<Result> Put([FromBody] DepartmentUpdateDto departmentUpdateDto)
        {
            var departmentId = await _departmentService.UpdateAsync(departmentUpdateDto);
            return await Result<Guid>.SuccessAsync(departmentId, "Department Updated Successfully");


        }
        [Authorize(Permissions.Departments.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var departmentId = await _departmentService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(departmentId, "Department Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(DepartmentRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _departmentService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var department in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        Name = department?.Name
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Department List");
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
