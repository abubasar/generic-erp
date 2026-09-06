using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Designation;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Designations
{
    public interface IDesignationService : IBaseService<Designation, DesignationCreationDto, DesignationUpdateDto, DesignationRequestModel, DesignationViewModel>
    {

    }
}
