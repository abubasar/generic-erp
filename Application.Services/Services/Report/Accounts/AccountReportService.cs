using Application.Core.Interfaces;
using Application.Services.Services.Accounts.Accounts;

namespace Application.Services.Services.Report.Accounts
{
    public class AccountReportService : IAccountReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountService _accountService;
        public AccountReportService(IUnitOfWork unitOfWork, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _accountService = accountService;
        }

    }
}
