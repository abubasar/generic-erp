using Application.Core.Enums;

namespace Application.Core.Interfaces
{
    /// <summary>
    /// The resolved tenant for the current request: id (from the JWT, via
    /// <see cref="Application.Core.Common.TenantScope"/>) plus the platform state
    /// loaded once per request by <c>TenantResolutionMiddleware</c> — enabled
    /// modules, quotas, subscription status, template.
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>Current tenant id, or <see cref="System.Guid.Empty"/> when there is no tenant.</summary>
        Guid TenantId { get; }

        /// <summary>True when a real tenant is in scope.</summary>
        bool HasTenant { get; }

        /// <summary>Enabled module keys (active <c>TenantModule</c> rows). Empty until resolved.</summary>
        IReadOnlySet<string> Modules { get; }

        /// <summary><c>BusinessTemplate</c> key the tenant was created from, e.g. "feed".</summary>
        string? BusinessTemplateKey { get; }

        /// <summary>Current subscription status; drives read-only / lockout behaviour.</summary>
        SubscriptionStatus Status { get; }

        /// <summary>Entitlement limits: max_users, max_branches, … Empty until resolved.</summary>
        IReadOnlyDictionary<string, int> Quotas { get; }

        /// <summary>True when <paramref name="moduleKey"/> is enabled for this tenant.</summary>
        bool HasModule(string moduleKey);
    }
}
