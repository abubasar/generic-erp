using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Configuration.CustomerWiseProductDiscount;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;

namespace Application.Services.Services.Configuration.CustomerWiseProductDiscounts
{
    public interface ICustomerWiseProductDiscountService : IBaseService<CustomerWiseProductDiscount, CustomerWiseProductDiscountCreationDto, CustomerWiseProductDiscountUpdateDto, CustomerWiseProductDiscountRequestModel, CustomerWiseProductDiscountViewModel>
    {
        new Task<AddUpdateResponseModel> AddAsync(CustomerWiseProductDiscountCreationDto creationDto);
        new Task<AddUpdateResponseModel> UpdateAsync(CustomerWiseProductDiscountUpdateDto updateDto);
        Task<CustomerWiseProductDiscountViewModel> GetByIdAsync(Guid id);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<CustomerInvoiceDiscountViewModel?> GetCustomerDiscountByProductId(Guid customerId, Guid productId);
        Task<List<ActiveCustomerWiseProductDiscountByCustomerIdViewModel>> GetActiveCustomerDiscountByCustomerId(Guid customerId);
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
