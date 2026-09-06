
using Application.Core.Entities;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;

namespace Application.Services.Services.Accounts.Suppliers
{
    public interface ISupplierService : IBaseService<Account, SupplierCreationDto, SupplierUpdateDto, SupplierRequestModel, SupplierViewModel>
    {
        Task<bool> SupplierExists(string name);

    }
}
