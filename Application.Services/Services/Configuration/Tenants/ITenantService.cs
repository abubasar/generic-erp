using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Tenant;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Tenants
{
    public interface ITenantService : IBaseService<Tenant, TenantCreationDto, TenantUpdateDto, TenantRequestModel, TenantViewModel>
    {
        Task<TenantViewModel> GetByIdAsync(Guid? id);
    }
}
