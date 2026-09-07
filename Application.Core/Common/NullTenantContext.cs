using System.Collections.Immutable;
using Application.Core.Enums;
using Application.Core.Interfaces;

namespace Application.Core.Common
{
    /// <summary>
    /// An <see cref="ITenantContext"/> with no tenant and no platform state. For
    /// design-time (<c>dotnet ef</c>), background work, and tests that only need a
    /// fixed tenant id.
    /// </summary>
    public class NullTenantContext(Guid tenantId = default) : ITenantContext
    {
        public Guid TenantId { get; } = tenantId;
        public bool HasTenant => TenantId != Guid.Empty;
        public IReadOnlySet<string> Modules => ImmutableHashSet<string>.Empty;
        public string? BusinessTemplateKey => null;
        public SubscriptionStatus Status => SubscriptionStatus.None;
        public IReadOnlyDictionary<string, int> Quotas => ImmutableDictionary<string, int>.Empty;
        public bool HasModule(string moduleKey) => false;
    }
}
