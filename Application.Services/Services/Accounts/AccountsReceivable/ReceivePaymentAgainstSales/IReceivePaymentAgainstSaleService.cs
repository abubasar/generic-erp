using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Accounts.AccountsReceivable.ReceivePaymentAgainstSales
{
    public interface IReceivePaymentAgainstSaleService : IBaseService<ReceivePaymentAgainstSale, ReceivePaymentAgainstSaleCreationDto, ReceivePaymentAgainstSaleUpdateDto, ReceivePaymentAgainstSaleRequestModel, ReceivePaymentAgainstSaleViewModel>
    {
        Task<ReceivePaymentAgainstSaleAggregatorModel> PrepareReceivePaymentAgainstSaleAggregatorModel(ReceivePaymentAgainstSaleRequestModel receivePaymentAgainstSaleRequest);
        Task<ReceivePaymentAgainstSaleViewModel> GetByIdAsync(Guid id);
        new Task<AddUpdateResponseModel> AddAsync(ReceivePaymentAgainstSaleCreationDto creationDto);
        Task<Guid> AddByDmsAppAsync(ReceivePaymentAgainstSaleCreationDtoForDmsApp receivePaymentAgainstSaleCreationDto);
        new Task<AddUpdateResponseModel> UpdateAsync(ReceivePaymentAgainstSaleUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<Guid> PostSingleFileAsync(ReceivePaymentAgainstSalePictureMappingCreationDto creationDto);
        Task<(Stream Stream, string FileName)?> GetFileById(Guid receivePaymentAgainstSaleId);
    }
}
