using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Machine;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.Machines
{
    public class MachineService : BaseService<Machine, MachineCreationDto, MachineUpdateDto, MachineRequestModel, MachineViewModel>, IMachineService
    {
        public MachineService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
