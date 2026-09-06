using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Region;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Regions
{
    public interface IRegionService : IBaseService<Region, RegionCreationDto, RegionUpdateDto, RegionRequestModel, RegionViewModel>
    {
    }
}
