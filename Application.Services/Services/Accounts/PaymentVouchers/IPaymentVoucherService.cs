using Application.Core.Entities;
using Application.Services.Dtos.Accounts.PaymentVoucher;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.PaymentVoucher;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Accounts.PaymentVouchers
{
    public interface IPaymentVoucherService : IBaseService<PaymentVoucher, PaymentVoucherCreationDto, PaymentVoucherUpdateDto, PaymentVoucherRequestModel, PaymentVoucherViewModel>
    {
        Task<PaymentVoucherAggregatorModel> PreparePaymentVoucherAggregatorModel(PaymentVoucherRequestModel request);
        Task<PaymentVoucherViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(PaymentVoucherCreationDto creationDto);
        new Task<Guid> UpdateAsync(PaymentVoucherUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
