using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.DeliveryPlace;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.DeliveryPlaces
{
    public class DeliveryPlaceService : BaseService<DeliveryPlace, DeliveryPlaceCreationDto, DeliveryPlaceUpdateDto, DeliveryPlaceRequestModel, DeliveryPlaceViewModel>, IDeliveryPlaceService
    {
        public DeliveryPlaceService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
