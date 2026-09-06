using Application.Core.Entities;
using Application.Services.Dtos.Configuration.DeliveryPlace;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.DeliveryPlaces
{
    public interface IDeliveryPlaceService : IBaseService<DeliveryPlace, DeliveryPlaceCreationDto, DeliveryPlaceUpdateDto, DeliveryPlaceRequestModel, DeliveryPlaceViewModel>
    {
    }
}
