using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Sale.SaleQuotation;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleQuotation;

namespace Application.Services.Services.Sale.SaleQuotations
{
    public interface ISaleQuotationService : IBaseService<SaleQuotation, SaleQuotationCreationDto, SaleQuotationUpdateDto, SaleQuotationRequestModel, SaleQuotationViewModel>
    {
        Task<SaleQuotationAggregatorModel> PrepareSaleQuotationAggregatorModel(SaleQuotationRequestModel request);
        Task<SaleQuotationViewModel> GetByIdAsync(Guid id);
        new Task<AddUpdateResponseModel> AddAsync(SaleQuotationCreationDto creationDto);
        new Task<AddUpdateResponseModel> UpdateAsync(SaleQuotationUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
