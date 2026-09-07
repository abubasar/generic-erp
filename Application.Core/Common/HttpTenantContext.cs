using System.Collections.Immutable;
using Application.Core.Enums;
using Application.Core.Interfaces;

namespace Application.Core.Common
{
    /// <summary>
    /// Request-scoped <see cref="ITenantContext"/>. The id comes from the ambient
    /// <see cref="TenantScope"/> (set by JwtMiddleware). The platform state
    /// (<see cref="Modules"/>, <see cref="Quotas"/>, …) is filled in once per
    /// request by <c>TenantResolutionMiddleware</c> via <see cref="Load"/>.
    /// </summary>
    public sealed class HttpTenantContext : ITenantContext
    {
        public Guid TenantId => TenantScope.CurrentTenantId;

        public bool HasTenant => TenantScope.HasTenant;

        public IReadOnlySet<string> Modules { get; private set; } = ImmutableHashSet<string>.Empty;

        public string? BusinessTemplateKey { get; private set; }

        public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.None;

        public IReadOnlyDictionary<string, int> Quotas { get; private set; } =
            ImmutableDictionary<string, int>.Empty;

        public bool HasModule(string moduleKey) => Modules.Contains(moduleKey);

        /// <summary>Called once per request by the resolution middleware.</summary>
        public void Load(
            IEnumerable<string> modules,
            string? businessTemplateKey,
            SubscriptionStatus status,
            IReadOnlyDictionary<string, int> quotas)
        {
            Modules = modules.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
            BusinessTemplateKey = businessTemplateKey;
            Status = status;
            Quotas = quotas;
        }
    }
}
