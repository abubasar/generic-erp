using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.InventoryType;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.InventoryTypes
{
    public class InventoryTypeService : BaseService<InventoryType, InventoryTypeCreationDto, InventoryTypeUpdateDto, InventoryTypeRequestModel, InventoryTypeViewModel>, IInventoryTypeService
    {
        public InventoryTypeService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext) { }

    }
}
