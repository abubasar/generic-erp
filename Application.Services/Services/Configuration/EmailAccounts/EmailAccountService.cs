using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.EmailAccount;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.EmailAccounts
{
    public class EmailAccountService : BaseService<EmailAccount, EmailAccountCreationDto, EmailAccountUpdateDto, EmailAccountRequestModel, EmailAccountViewModel>, IEmailAccountService
    {
        public EmailAccountService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
