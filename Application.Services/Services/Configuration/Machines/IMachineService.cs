using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Machine;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Machines
{
    public interface IMachineService : IBaseService<Machine, MachineCreationDto, MachineUpdateDto, MachineRequestModel, MachineViewModel>
    {
    }
}
