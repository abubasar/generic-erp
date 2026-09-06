using Application.Core.Entities;
using Application.Services.Dtos.Purchase.PurchaseInvoice;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseInvoice;
using Application.Services.ViewModels.Report.Purchase;

namespace Application.Services.Services.Purchase.PurchaseInvoices
{
    public interface IPurchaseInvoiceService : IBaseService<PurchaseInvoice, PurchaseInvoiceCreationDto, PurchaseInvoiceUpdateDto, PurchaseInvoiceRequestModel, PurchaseInvoiceViewModel>
    {
        Task<PurchaseInvoiceAggregatorModel> PreparePurchaseInvoiceAggregatorModel(PurchaseInvoiceRequestModel request);
        Task<PurchaseInvoiceViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(PurchaseInvoiceCreationDto creationDto);
        new Task<Guid> UpdateAsync(PurchaseInvoiceUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<Guid> ResetAssociatedAdvanceAmountAsync(Guid id);
        Task<List<PurchaseItemViewModel>> SupplierWisePurchaseItemSearchAsync(PurchaseInvoiceRequestModel request);
        Task<List<PurchaseInvoiceItemViewModel>> PurchaseInvoiceItemSearchAsync(PurchaseInvoiceRequestModel request);
        Task<PurchaseReportViewModel> PurchaseReportSearchAsync(PurchaseInvoiceRequestModel request);
    }
}
