using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.ProductType;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.ProductTypes
{
    public class ProductTypeService : BaseService<ProductType, ProductTypeCreationDto, ProductTypeUpdateDto, ProductTypeRequestModel, ProductTypeViewModel>, IProductTypeService
    {

        public ProductTypeService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext) { }


    }
}
