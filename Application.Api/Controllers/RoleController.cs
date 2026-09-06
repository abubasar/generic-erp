
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Role;
using Application.Services.SearchRequestModels.Auth;
using Application.Services.Services.Auth.Roles;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Auth;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleService _roleService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public RoleController(IRoleService roleService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _roleService = roleService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
        }
        // [Authorize(Policy = Permissions.Roles.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<RoleViewModel>, int>>> Search(RoleRequestModel request)
        {
            return await Result<Tuple<List<RoleViewModel>, int>>.SuccessAsync(await _roleService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Roles.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] RoleCreationDto roleCreationDto)
        {
            var roleId = await _roleService.AddAsync(roleCreationDto);
            return await Result<Guid>.SuccessAsync(roleId, "Role Added Successfully");
        }
        [Authorize(Permissions.Roles.Edit)]
        [HttpPost("/api/role/update")]
        public virtual async Task<Result> Put([FromBody] RoleUpdateDto roleUpdateDto)
        {
            var roleId = await _roleService.UpdateAsync(roleUpdateDto);
            return await Result<Guid>.SuccessAsync(roleId, "Role Updated Successfully");
        }
        [Authorize(Permissions.Roles.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var roleId = await _roleService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(roleId, "Role Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(RoleRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _roleService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var role in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        role?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Role List");
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
