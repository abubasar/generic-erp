using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Store;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Stores
{
    public interface IStoreService : IBaseService<Store, StoreCreationDto, StoreUpdateDto, StoreRequestModel, StoreViewModel>
    {
    }
}
