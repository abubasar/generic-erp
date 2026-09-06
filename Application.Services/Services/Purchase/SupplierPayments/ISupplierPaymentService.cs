using Application.Core.Entities;
using Application.Services.Dtos.Purchase.SupplierPayment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.SupplierPayment;

namespace Application.Services.Services.Purchase.SupplierPayments
{
    public interface ISupplierPaymentService : IBaseService<SupplierPayment, SupplierPaymentCreationDto, SupplierPaymentUpdateDto, SupplierPaymentRequestModel, SupplierPaymentViewModel>
    {
        Task<SupplierPaymentAggregatorModel> PrepareSupplierPaymentAggregatorModel(SupplierPaymentRequestModel request);
        Task<SupplierPaymentViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(SupplierPaymentCreationDto creationDto);
        new Task<Guid> UpdateAsync(SupplierPaymentUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
