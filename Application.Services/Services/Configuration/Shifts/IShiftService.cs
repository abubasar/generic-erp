using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Shift;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Shifts
{
    public interface IShiftService : IBaseService<Shift, ShiftCreationDto, ShiftUpdateDto, ShiftRequestModel, ShiftViewModel>
    {
    }
}
