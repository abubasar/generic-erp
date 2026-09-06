using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Accounts.FundTransfer;
using Application.Services.Dtos.Purchase.LCCostEntry;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.FundTransfer;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.LCCostEntry;

namespace Application.Services.Services.Purchase.LCCostEntries
{
    public interface ILCCostEntryService : IBaseService<LccostEntry, LCCostEntryCreationDto, LCCostEntryUpdateDto, LCCostEntryRequestModel, LCCostEntryViewModel>
    {
        Task<LCCostEntryAggregatorModel> PrepareLCCostEntryAggregatorModel(LCCostEntryRequestModel request);
        Task<LCCostEntryViewModel> GetByIdAsync(Guid id);
        Task<List<LcCostEntryDetailAgainstPoViewModel>> GetLcCostEntriesByPoAsync(Guid purchaseOrderId);
        new Task<Guid> AddAsync(LCCostEntryCreationDto lCCostEntryCreationDto);
        new Task<Guid> UpdateAsync(LCCostEntryUpdateDto lCCostEntryUpdateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
