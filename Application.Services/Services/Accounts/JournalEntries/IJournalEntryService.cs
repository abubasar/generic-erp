using Application.Core.Entities;
using Application.Services.Dtos.Accounts.JournalEntry;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Accounts.JournalEntries
{
    public interface IJournalEntryService : IBaseService<JournalEntry, JournalEntryCreationDto, JournalEntryUpdateDto, JournalEntryRequestModel, JournalEntryViewModel>
    {
        Task<JournalEntryViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(JournalEntryCreationDto creationDto);
        new Task<Guid> UpdateAsync(JournalEntryUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
