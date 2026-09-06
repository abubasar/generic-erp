using Application.Core.Entities;
using Application.Services.Dtos.Accounts.ReceiveVoucher;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.ReceiveVoucher;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Accounts.ReceiveVouchers
{
    public interface IReceiveVoucherService : IBaseService<ReceiveVoucher, ReceiveVoucherCreationDto, ReceiveVoucherUpdateDto, ReceiveVoucherRequestModel, ReceiveVoucherViewModel>
    {
        Task<ReceiveVoucherAggregatorModel> PrepareReceiveVoucherAggregatorModel(ReceiveVoucherRequestModel request);
        Task<ReceiveVoucherViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(ReceiveVoucherCreationDto creationDto);
        new Task<Guid> UpdateAsync(ReceiveVoucherUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
