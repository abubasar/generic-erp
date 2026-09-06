using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Zone;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.Zones
{
    public class ZoneService : BaseService<Zone, ZoneCreationDto, ZoneUpdateDto, ZoneRequestModel, ZoneViewModel>, IZoneService
    {

        public ZoneService(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork) : base(unitOfWork, mapper, workContext) { }

    }
}
