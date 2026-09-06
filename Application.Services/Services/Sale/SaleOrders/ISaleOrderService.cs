using Application.Core.Entities;
using Application.Services.Dtos.Sale.SaleOrder;
using Application.Services.SearchRequestModels.Report;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Report.Sales;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleOrder;

namespace Application.Services.Services.Sale.SaleOrders
{
    public interface ISaleOrderService : IBaseService<SaleOrder, SaleOrderCreationDto, SaleOrderUpdateDto, SaleOrderRequestModel, SaleOrderViewModel>
    {
        Task<SaleOrderAggregatorModel> PrepareSaleOrderAggregatorModel(SaleOrderRequestModel request);
        new Task<Guid> AddAsync(SaleOrderCreationDto creationDto);
        new Task<Guid> UpdateAsync(SaleOrderUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<bool> CloseAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<SaleOrderViewModel> GetByIdAsync(Guid id);
        Task<List<SalesOrderItemWithoutDeliveryItemViewModel>> GetSaleOrderedDataWithoutDeliveryItemAsync(SalesOrderItemRequestModel request);
    }
}
