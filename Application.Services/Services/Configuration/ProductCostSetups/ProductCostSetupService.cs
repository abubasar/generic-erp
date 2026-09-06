using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.ProductCostSetup;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.ProductCostSetups
{
    public class ProductCostSetupService : BaseService<ProductCostSetup, ProductCostSetupCreationDto, ProductCostSetupUpdateDto, ProductCostSetupRequestModel, ProductCostSetupViewModel>, IProductCostSetupService
    {
        public ProductCostSetupService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
