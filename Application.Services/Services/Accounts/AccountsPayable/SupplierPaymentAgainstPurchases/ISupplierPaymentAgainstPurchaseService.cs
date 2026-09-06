using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase;
using Application.Services.SearchRequestModels.Accounts.AccountsPayable;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsPayable;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Accounts.AccountsPayable.SupplierPaymentAgainstPurchases
{
    public interface ISupplierPaymentAgainstPurchaseService : IBaseService<SupplierPaymentAgainstPurchase, SupplierPaymentAgainstPurchaseCreationDto, SupplierPaymentAgainstPurchaseUpdateDto, SupplierPaymentAgainstPurchaseRequestModel, SupplierPaymentAgainstPurchaseViewModel>
    {
        Task<SupplierPaymentAgainstPurchaseViewModel> GetByIdAsync(Guid id);
        new Task<AddUpdateResponseModel> AddAsync(SupplierPaymentAgainstPurchaseCreationDto creationDto);
        new Task<AddUpdateResponseModel> UpdateAsync(SupplierPaymentAgainstPurchaseUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
