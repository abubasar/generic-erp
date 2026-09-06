
using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Product;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Products
{
    public interface IProductService : IBaseService<Product, ProductCreationDto, ProductUpdateDto, ProductRequestModel, ProductViewModel>
    {
        Task<bool> ProductExists(string name);
    }
}
