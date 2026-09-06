using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Shift;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.Shifts
{
    public class ShiftService : BaseService<Shift, ShiftCreationDto, ShiftUpdateDto, ShiftRequestModel, ShiftViewModel>, IShiftService
    {
        public ShiftService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
