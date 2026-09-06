using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Shift;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.Shifts;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShiftService _shiftService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public ShiftController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IShiftService shiftService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _shiftService = shiftService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.Shifts.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ShiftViewModel>, int>>> Search(ShiftRequestModel request)
        {
            return await Result<Tuple<List<ShiftViewModel>, int>>.SuccessAsync(await _shiftService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Shifts.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ShiftCreationDto shiftCreationDto)
        {
            var shiftId = await _shiftService.AddAsync(shiftCreationDto);
            return await Result<Guid>.SuccessAsync(shiftId, "Shift Added Successfully");
        }
        [Authorize(Permissions.Shifts.Edit)]
        [HttpPost("/api/shift/update")]
        public virtual async Task<Result> Put([FromBody] ShiftUpdateDto shiftUpdateDto)
        {
            var shiftId = await _shiftService.UpdateAsync(shiftUpdateDto);
            return await Result<Guid>.SuccessAsync(shiftId, "Shift Updated Successfully");
        }
        [Authorize(Permissions.Shifts.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var shiftId = await _shiftService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(shiftId, "Shift Deleted Successfully");
        }
    }
}
