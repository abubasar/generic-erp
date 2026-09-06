using Application.Core.Entities;
using Application.Services.Dtos.Purchase.StockAdjustment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Inventory.StockAdjustments
{
    public interface IStockAdjustmentService : IBaseService<StockAdjustment, StockAdjustmentCreationDto, StockAdjustmentUpdateDto, StockAdjustmentRequestModel, StockAdjustmentViewModel>
    {
        Task<StockAdjustmentViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(StockAdjustmentCreationDto creationDto);
        new Task<Guid> UpdateAsync(StockAdjustmentUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
