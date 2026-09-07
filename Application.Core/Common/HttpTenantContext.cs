using Application.Core.Interfaces;

namespace Application.Core.Common
{
    /// <summary>
    /// DI-friendly view of the ambient <see cref="TenantScope"/> for the application
    /// layer. Phase 1 grows <see cref="ITenantContext"/> with enabled modules,
    /// quotas and subscription state; for now it is just the id.
    /// </summary>
    public sealed class HttpTenantContext : ITenantContext
    {
        public Guid TenantId => TenantScope.CurrentTenantId;

        public bool HasTenant => TenantScope.HasTenant;
    }
}
