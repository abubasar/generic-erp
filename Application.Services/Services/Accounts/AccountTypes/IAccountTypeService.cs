using Application.Core.Entities;
using Application.Services.Dtos.Accounts.AccountType;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;

namespace Application.Services.Services.Accounts.AccountTypes
{
    public interface IAccountTypeService : IBaseService<AccountType, AccountTypeCreationDto, AccountTypeUpdateDto, AccountTypeRequestModel, AccountTypeViewModel>
    {

    }
}
