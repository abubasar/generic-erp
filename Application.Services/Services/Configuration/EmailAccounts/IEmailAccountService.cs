using Application.Core.Entities;
using Application.Services.Dtos.Configuration.EmailAccount;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.EmailAccounts
{
    public interface IEmailAccountService : IBaseService<EmailAccount, EmailAccountCreationDto, EmailAccountUpdateDto, EmailAccountRequestModel, EmailAccountViewModel>
    {
    }
}
