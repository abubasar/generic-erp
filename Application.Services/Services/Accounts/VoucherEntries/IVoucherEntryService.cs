using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Accounts.VoucherEntry;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Accounts.VoucherEntries
{
    public interface IVoucherEntryService : IBaseService<VoucherEntry, VoucherEntryCreationDto, VoucherEntryUpdateDto, VoucherEntryRequestModel, VoucherEntryViewModel>
    {
        Task<VoucherEntryViewModel> GetByIdAsync(Guid id);
        new Task<AddUpdateResponseModel> AddAsync(VoucherEntryCreationDto creationDto);
        new Task<AddUpdateResponseModel> UpdateAsync(VoucherEntryUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
