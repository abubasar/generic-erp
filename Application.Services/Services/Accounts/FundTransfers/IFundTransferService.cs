using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Accounts.FundTransfer;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.FundTransfer;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Accounts.FundTransfers
{
    public interface IFundTransferService : IBaseService<FundTransfer, FundTransferCreationDto, FundTransferUpdateDto, FundTransferRequestModel, FundTransferViewModel>
    {
        Task<FundTransferAggregatorModel> PrepareFundTransferAggregatorModel(FundTransferRequestModel request);
        Task<FundTransferViewModel> GetByIdAsync(Guid id);
        new Task<AddUpdateResponseModel> AddAsync(FundTransferCreationDto creationDto);
        new Task<AddUpdateResponseModel> UpdateAsync(FundTransferUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
