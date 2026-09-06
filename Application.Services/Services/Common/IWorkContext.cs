using Application.Core.Entities;

namespace Application.Services.Services.Common
{
    public interface IWorkContext
    {
        string? GetUserId();
        string? GetUserName();
        Guid? GetTenantId();
        FinancialYear GetCurrentFinancialYear();
    }
}
