using Application.Core.Entities;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.ReceivePayment;
using Application.Services.ViewModels.Purchase;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Services.Accounts.AccountsReceivable.ReceivePayments
{
    public interface IReceivePaymentService : IBaseService<ReceivePayment, ReceivePaymentCreationDto, ReceivePaymentUpdateDto, ReceivePaymentRequestModel, ReceivePaymentViewModel>
    {
        Task<ReceivePaymentAggregatorModel> PrepareReceivePaymentAggregatorModel(ReceivePaymentRequestModel request);
        Task<ReceivePaymentViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(ReceivePaymentCreationDto creationDto);
        Task<Guid> AddByDmsAppAsync(ReceivePaymentCreationDtoForDmsApp receivePaymentCreationDto);
        new Task<Guid> UpdateAsync(ReceivePaymentUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<bool> SendToCustomerAsync(Guid id);
        Task<Guid> PostSingleFileAsync(ReceivePaymentPictureMappingCreationDto creationDto);
        Task AddReceivePaymentPictureMappingAsync(List<ReceivePaymentPictureMappingCreationDto> receivePaymentPictureMappingCreationDto);
        Task<(Stream Stream, string FileName)?> GetFileById(Guid receivePaymentId);
    }
}
