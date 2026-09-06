using Application.Core.Entities;
using Application.Services.Dtos.Purchase.VendorQuotation;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Purchase.VendorQuotations
{
    public interface IVendorQuotationService : IBaseService<VendorQuotation, VendorQuotationCreationDto, VendorQuotationUpdateDto, VendorQuotationRequestModel, VendorQuotationViewModel>
    {
        Task<VendorQuotationViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(VendorQuotationCreationDto creationDto);
        new Task<Guid> UpdateAsync(VendorQuotationUpdateDto updateDto);
        Task<bool> ApproveQuotationAsync(Guid vendorQuotationId, string requisitionNo);
        Task<bool> UnpostQuotationAsync(string requisitionNo);
    }
}
