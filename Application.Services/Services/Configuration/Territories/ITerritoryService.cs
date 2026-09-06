using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Territory;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Territories
{
    public interface ITerritoryService : IBaseService<Territory, TerritoryCreationDto, TerritoryUpdateDto, TerritoryRequestModel, TerritoryViewModel>
    {

    }
}
