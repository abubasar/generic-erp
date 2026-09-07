namespace Application.Core.Interfaces
{
    /// <summary>
    /// An <see cref="ITenantScoped"/> entity that also exposes rows shared by every
    /// tenant (<c>TenantId == TenancyConstants.SystemTenantId</c>). The EF query
    /// filter for these types matches the current tenant's rows OR the shared rows.
    /// Only <c>Account</c> uses this today — see <see cref="Application.Core.Common.TenancyConstants"/>.
    /// </summary>
    public interface ITenantSharable : ITenantScoped
    {
    }
}
