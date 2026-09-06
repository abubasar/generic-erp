using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Generic;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Generics
{
    public interface IGenericServicecs : IBaseService<Generic, GenericCreationDto, GenericUpdateDto, GenericRequestModel, GenericViewModel>
    {
    }
}
