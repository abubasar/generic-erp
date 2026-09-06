using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Machine;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Machines;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMachineService _machineService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public MachineController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IMachineService machineService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _machineService = machineService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Machines.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<MachineViewModel>, int>>> Search(MachineRequestModel request)
        {
            return await Result<Tuple<List<MachineViewModel>, int>>.SuccessAsync(await _machineService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Machines.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] MachineCreationDto machineCreationDto)
        {
            var machineId = await _machineService.AddAsync(machineCreationDto);
            return await Result<Guid>.SuccessAsync(machineId, "Machine Added Successfully");
        }
        [Authorize(Permissions.Machines.Edit)]
        [HttpPost("/api/machine/update")]
        public virtual async Task<Result> Put([FromBody] MachineUpdateDto machineUpdateDto)
        {
            var machineId = await _machineService.UpdateAsync(machineUpdateDto);
            return await Result<Guid>.SuccessAsync(machineId, "Machine Updated Successfully");
        }
        [Authorize(Permissions.Machines.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var machineId = await _machineService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(machineId, "Machine Deleted Successfully");
        }
    }
}
