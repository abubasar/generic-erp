using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Generic;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.Generics
{
    public class GenericService : BaseService<Generic, GenericCreationDto, GenericUpdateDto, GenericRequestModel, GenericViewModel>, IGenericServicecs
    {
        public GenericService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
