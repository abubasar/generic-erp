using Application.Core.Entities;
using Application.Services.Dtos.Purchase.PurchaseReturn;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseReturn;

namespace Application.Services.Services.Purchase.PurchaseReturns
{
    public interface IPurchaseReturnService : IBaseService<PurchaseReturn, PurchaseReturnCreationDto, PurchaseReturnUpdateDto, PurchaseReturnRequestModel, PurchaseReturnViewModel>
    {
        Task<PurchaseReturnAggregatorModel> PreparePurchaseReturnAggregatorModel(PurchaseReturnRequestModel request);
        Task<PurchaseReturnViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(PurchaseReturnCreationDto creationDto);
        new Task<Guid> UpdateAsync(PurchaseReturnUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
