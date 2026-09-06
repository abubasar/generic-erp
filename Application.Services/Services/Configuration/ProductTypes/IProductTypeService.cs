
using Application.Core.Entities;
using Application.Services.Dtos.Configuration.ProductType;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.ProductTypes
{
    public interface IProductTypeService : IBaseService<ProductType, ProductTypeCreationDto, ProductTypeUpdateDto, ProductTypeRequestModel, ProductTypeViewModel>
    {

    }
}
