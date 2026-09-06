using Application.Core.Entities;
using Application.Services.Dtos.Purchase.PurchaseRequisition;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Purchase.PurchaseRequisitions
{
    public interface IPurchaseRequisitionService : IBaseService<PurchaseRequisition, PurchaseRequisitionCreationDto, PurchaseRequisitionUpdateDto, PurchaseRequisitionRequestModel, PurchaseRequisitionViewModel>
    {
        Task<PurchaseRequisitionViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(PurchaseRequisitionCreationDto creationDto);
        new Task<Guid> UpdateAsync(PurchaseRequisitionUpdateDto updateDto);
        Task<Guid> PrepareRfqAsync(PurchaseRequisitionUpdateDto purchaseRequisitionUpdateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<int> SendRFQtoSelectedSupplier(SendRFQtoSelectedSupplierDto sendRFQtoSelectedSupplierDto);
        Task<List<RFQSentSupplier>> GetRfqSentSuppliersByRequisitionId(Guid id);


    }
}
