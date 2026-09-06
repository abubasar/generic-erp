using Application.Core.Entities;
using Application.Services.Dtos.Sale.SaleReturn;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleReturn;

namespace Application.Services.Services.Sale.SaleReturns
{
    public interface ISaleReturnService : IBaseService<SaleReturn, SaleReturnCreationDto, SaleReturnUpdateDto, SaleReturnRequestModel, SaleReturnViewModel>
    {
        Task<SaleReturnAggregatorModel> PrepareSaleReturnAggregatorModel(SaleReturnRequestModel request);
        Task<SaleReturnViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(SaleReturnCreationDto creationDto);
        new Task<Guid> UpdateAsync(SaleReturnUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
