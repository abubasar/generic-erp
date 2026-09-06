using Application.Core.Entities;
using Application.Services.Dtos.Production.Production;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Production.Production;
using Application.Services.ViewModels.Purchase;
using System.Dynamic;

namespace Application.Services.Services.Productions.Productions
{
    public interface IProductionService : IBaseService<Production, ProductionCreationDto, ProductionUpdateDto, ProductionRequestModel, ProductionViewModel>
    {
        Task<ProductionAggregatorModel> PrepareProductionAggregatorModel(ProductionRequestModel request);
        Task<ProductionViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(ProductionCreationDto creationDto);
        new Task<Guid> UpdateAsync(ProductionUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<List<ExpandoObject>> DayWiseProductionSummaryRowColumnDynamic(ProductionRequestModel request);
        Task<List<ExpandoObject>> DayWiseConsumptionQtyRowColumnDynamic(ConsumptionRequestModel request);
        Task<List<ExpandoObject>> DayWiseConsumptionRateRowColumnDynamic(ConsumptionRequestModel request);
        Task<Dictionary<string, List<ExpandoObject>>> SalesMoWise(ProductionRequestModel request);
        Task<Dictionary<string, List<ExpandoObject>>> SalesCustomerWise(ProductionRequestModel request);
    }
}
