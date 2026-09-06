using Application.Core.Entities;
using Application.Services.Dtos.Configuration.CostCenter;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.CostCenters
{
    public interface ICostCenterService : IBaseService<CostCenter, CostCenterCreationDto, CostCenterUpdateDto, CostCenterRequestModel, CostCenterViewModel>
    {

    }
}
