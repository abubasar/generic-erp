using Application.Core.Entities;
using Application.Services.Dtos.Production.BillOfMaterial;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Productions.BillOfMaterials
{
    public interface IBillOfMaterialService : IBaseService<BillOfMaterial, BillOfMaterialCreationDto, BillOfMaterialUpdateDto, BillOfMaterialRequestModel, BillOfMaterialViewModel>
    {
        Task<BillOfMaterialViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(BillOfMaterialCreationDto creationDto);
        new Task<Guid> UpdateAsync(BillOfMaterialUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
