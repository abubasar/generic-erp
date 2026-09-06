using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.CostCenter;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.CostCenters
{
    public class CostCenterService : BaseService<CostCenter, CostCenterCreationDto, CostCenterUpdateDto, CostCenterRequestModel, CostCenterViewModel>, ICostCenterService
    {

        public CostCenterService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext) { }

    }
}
