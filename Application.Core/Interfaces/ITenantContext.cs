namespace Application.Core.Interfaces
{
    /// <summary>
    /// The resolved tenant for the current request. In Phase 0 this only carries the
    /// tenant id; Phase 1 adds enabled modules, quotas and subscription state.
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>Current tenant id, or <see cref="Guid.Empty"/> when there is no tenant
        /// (unauthenticated requests, login, design-time).</summary>
        Guid TenantId { get; }

        /// <summary>True when a real tenant is in scope.</summary>
        bool HasTenant { get; }
    }
}
