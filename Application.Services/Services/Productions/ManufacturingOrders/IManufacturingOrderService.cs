using Application.Core.Entities;
using Application.Services.Dtos.Production.ManufacturingOrder;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Production.ManufacturingOrder;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Productions.ManufacturingOrders
{
    public interface IManufacturingOrderService : IBaseService<ManufacturingOrder, ManufacturingOrderCreationDto, ManufacturingOrderUpdateDto, ManufacturingOrderRequestModel, ManufacturingOrderViewModel>
    {
        Task<ManufacturingOrderAggregatorModel> PrepareManufacturingOrderAggregatorModel(ManufacturingOrderRequestModel request);
        Task<ManufacturingOrderViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(ManufacturingOrderCreationDto creationDto);
        new Task<Guid> UpdateAsync(ManufacturingOrderUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
