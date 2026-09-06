using Application.Core.Entities;
using Application.Services.Dtos.Sale.DeliveryNote;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.DeliveryNote;

namespace Application.Services.Services.Sale.DeliveryNotes
{
    public interface IDeliveryNoteService : IBaseService<DeliveryNote, DeliveryNoteCreationDto, DeliveryNoteUpdateDto, DeliveryNoteRequestModel, DeliveryNoteViewModel>
    {
        Task<DeliveryNoteAggregatorModel> PrepareDeliveryNoteAggregatorModel(DeliveryNoteRequestModel request);
        Task<DeliveryNoteViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(DeliveryNoteCreationDto creationDto);
        new Task<Guid> UpdateAsync(DeliveryNoteUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
