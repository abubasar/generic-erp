using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Designation;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.Designations
{
    public class DesignationService : BaseService<Designation, DesignationCreationDto, DesignationUpdateDto, DesignationRequestModel, DesignationViewModel>, IDesignationService
    {
        public DesignationService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext) { }
    }
}
