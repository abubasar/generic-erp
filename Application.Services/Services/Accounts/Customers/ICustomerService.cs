using Application.Core.Entities;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;

namespace Application.Services.Services.Accounts.Customers
{
    public interface ICustomerService : IBaseService<Account, CustomerCreationDto, CustomerUpdateDto, CustomerRequestModel, CustomerViewModel>
    {
        Task<(decimal creditLimit, decimal balance, Guid? customerMarketingOfficerId, Guid? customerTerritoryId)> FindCustomerCreditLimitAndBalance(Guid customerId);
        Task<decimal> FindCustomerBalance(Guid customerId);
        Task<bool> CustomerExists(string? id, string name, string contactNo);
        Task SendSmsToActiveCustomers();
    }
}
