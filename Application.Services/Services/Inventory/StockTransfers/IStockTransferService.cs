using Application.Core.Entities;
using Application.Services.Dtos.Inventory.StockTransfer;
using Application.Services.SearchRequestModels.Inventory;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Inventory;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Inventory.StockTransfers
{
    public interface IStockTransferService : IBaseService<StockTransfer, StockTransferCreationDto, StockTransferUpdateDto, StockTransferRequestModel, StockTransferViewModel>
    {
        Task<StockTransferViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(StockTransferCreationDto creationDto);
        new Task<Guid> UpdateAsync(StockTransferUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
