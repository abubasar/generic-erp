using Application.Core.Entities;
using Application.Services.Dtos.Purchase.PurchaseOrder;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseOrder;

namespace Application.Services.Services.Purchase.PurchaseOrders
{
    public interface IPurchaseOrderService : IBaseService<PurchaseOrder, PurchaseOrderCreationDto, PurchaseOrderUpdateDto, PurchaseOrderRequestModel, PurchaseOrderViewModel>
    {
        Task<PurchaseOrderAggregatorModel> PreparePurchaseOrderAggregatorModel(PurchaseOrderRequestModel request);
        Task<PurchaseOrderViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(PurchaseOrderCreationDto creationDto);
        new Task<Guid> UpdateAsync(PurchaseOrderUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<bool> ReadyForGrnAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<bool> SendToSupplierAsync(Guid id);
        Task<bool> CloseAsync(Guid id);
        Task<List<SupplierTransactionAgainstPoViewModel>> GetSupplierTransactionsAgainstPo(Guid id);
        Task<LastPoDetailsViewModel> SearchLastPoDetailsAsync(Guid supplierId, Guid productId);
    }
}
