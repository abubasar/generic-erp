using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountType;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using AutoMapper;

namespace Application.Services.Services.Accounts.AccountTypes
{
    public class AccountTypeService : BaseService<AccountType, AccountTypeCreationDto, AccountTypeUpdateDto, AccountTypeRequestModel, AccountTypeViewModel>, IAccountTypeService
    {
        public AccountTypeService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext) { }

    }
}