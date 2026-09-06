using Application.Core.Entities;
using Application.Services.Dtos.Purchase.PoPriceAdjustmentAfterGrn;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PoPriceAdjustmentAfterGrn;

namespace Application.Services.Services.Purchase.PoPriceAdjustmentAfterGrns
{
    public interface IPoPriceAdjustmentAfterGrnService : IBaseService<PoPriceAdjustmentAfterGrn, PoPriceAdjustmentAfterGrnCreationDto, PoPriceAdjustmentAfterGrnUpdateDto, PoPriceAdjustmentAfterGrnRequestModel, PoPriceAdjustmentAfterGrnViewModel>
    {
        Task<PoPriceAdjustmentAfterGrnAggregatorModel> PreparePoPriceAdjustmentAfterGrnAggregatorModel(PoPriceAdjustmentAfterGrnRequestModel request);
        Task<PoPriceAdjustmentAfterGrnViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(PoPriceAdjustmentAfterGrnCreationDto creationDto);
        new Task<Guid> UpdateAsync(PoPriceAdjustmentAfterGrnUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
