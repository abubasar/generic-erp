using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Employee;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Employees;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeService _employeeService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public EmployeeController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IEmployeeService employeeService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _employeeService = employeeService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Employees.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<EmployeeViewModel>, int>>> Search(EmployeeRequestModel request)
        {
            return await Result<Tuple<List<EmployeeViewModel>, int>>.SuccessAsync(await _employeeService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Employees.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] EmployeeCreationDto employeeCreationDto)
        {
            var employeeId = await _employeeService.AddAsync(employeeCreationDto);
            return await Result<Guid>.SuccessAsync(employeeId, "Employee Added Successfully");
        }
        [Authorize(Permissions.Employees.Edit)]
        [HttpPost("/api/employee/update")]
        public virtual async Task<Result> Put([FromBody] EmployeeUpdateDto employeeUpdateDto)
        {
            var employeeId = await _employeeService.UpdateAsync(employeeUpdateDto);
            return await Result<Guid>.SuccessAsync(employeeId, "Employee Updated Successfully");
        }
        [Authorize(Permissions.Employees.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var employeeId = await _employeeService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(employeeId, "Employee Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(EmployeeRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _employeeService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name", "Job ID", "Designation", "Department", "Job Location", "Joining Date", "Contact No", "Email", "Adress", "Gender", "Blood Group", "Marital Status", "DOB" };
                List<float> columnWidths = new List<float> { 3f, 8f, 9f, 9f, 8f, 6f, 7f, 9f, 7f, 7f, 6f, 6f, 6f, 7f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var employee in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        Name = employee?.FirstName + " " + employee?.LastName,
                        JobId = employee?.EmployeeIdNo,
                        Designation = employee?.Designation?.Name,
                        Department = employee?.Department?.Name,
                        JobLocation = employee?.JobLocation?.Name,
                        JoiningDate = employee?.JoiningDate.ToString("dd/MM/yyyy"),
                        employee?.ContactNo,
                        employee?.Email,
                        employee?.Address,
                        Gender = Enum.GetName(typeof(Gender), employee!.Gender)?.Replace("_", " "),
                        BllodGroup = Enum.GetName(typeof(BloodGroup), employee!.BloodGroup)?.Replace("_", " "),
                        MaritalStatus = Enum.GetName(typeof(MaritalStatus), employee!.MaritalStatus)?.Replace("_", " "),
                        DOB = employee?.DateOfBirth.ToString("dd/MM/yyyy")
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Employee List", true);
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
