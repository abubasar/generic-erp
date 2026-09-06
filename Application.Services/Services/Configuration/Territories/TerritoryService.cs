using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Territory;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.Territories
{
    public class TerritoryService : BaseService<Territory, TerritoryCreationDto, TerritoryUpdateDto, TerritoryRequestModel, TerritoryViewModel>, ITerritoryService
    {
        public TerritoryService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
