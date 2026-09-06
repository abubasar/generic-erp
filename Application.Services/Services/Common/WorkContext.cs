using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Core.PermissionHelpers;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Services.Common
{
    public class WorkContext : IWorkContext
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        public WorkContext(IHttpContextAccessor contextAccessor, IUnitOfWork unitOfWork)
        {
            _contextAccessor = contextAccessor;
            _unitOfWork = unitOfWork;
        }

        public string? GetUserName()
        {
            return _contextAccessor.HttpContext?.GetUserName();
        }
        public string? GetUserId()
        {
            return _contextAccessor.HttpContext?.GetUserId();
        }
        public Guid? GetTenantId()
        {
            return _contextAccessor.HttpContext?.GetTenantId();
        }
        public FinancialYear GetCurrentFinancialYear()
        {
            var financialYear = _unitOfWork.Repository<FinancialYear>().TableNoTracking().Where(x => x.IsActive)?.SingleOrDefault();
            if (financialYear is null) throw new BadRequestException("Financial Year not found!!!");
            return financialYear;
        }

    }
}
