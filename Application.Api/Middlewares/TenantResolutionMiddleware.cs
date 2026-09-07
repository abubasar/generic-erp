using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Middlewares
{
    /// <summary>
    /// Runs after <see cref="JwtMiddleware"/>. When a tenant is in scope, loads its
    /// platform state once — enabled modules (Core modules + active
    /// <c>TenantModule</c> rows), subscription status, entitlement quotas,
    /// business template — into the request-scoped <see cref="HttpTenantContext"/>.
    /// </summary>
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context, HttpTenantContext tenantContext, DataContext db)
        {
            var tenantId = TenantScope.CurrentTenantId;
            if (tenantId != Guid.Empty)
            {
                var now = DateTime.UtcNow;

                var coreModules = await db.Modules
                    .Where(m => m.Category == "Core")
                    .Select(m => m.Key)
                    .ToListAsync();

                var tenantModules = await db.TenantModules
                    .Where(tm => tm.TenantId == tenantId
                                 && tm.Status == "Active"
                                 && (tm.ExpiresOn == null || tm.ExpiresOn > now))
                    .Select(tm => tm.ModuleKey)
                    .ToListAsync();

                var quotas = await db.Entitlements
                    .Where(en => en.TenantId == tenantId)
                    .ToDictionaryAsync(en => en.Key, en => en.Limit);

                var templateKey = await db.Tenants
                    .Where(t => t.Id == tenantId)
                    .Select(t => t.BusinessTemplateKey)
                    .FirstOrDefaultAsync();

                var subStatus = await db.Subscriptions
                    .Where(s => s.TenantId == tenantId)
                    .OrderByDescending(s => s.PeriodEnd)
                    .Select(s => s.Status)
                    .FirstOrDefaultAsync();

                tenantContext.Load(
                    coreModules.Concat(tenantModules),
                    templateKey,
                    Enum.TryParse<SubscriptionStatus>(subStatus, ignoreCase: true, out var st)
                        ? st
                        : SubscriptionStatus.None,
                    quotas);
            }

            await _next(context);
        }
    }
}
