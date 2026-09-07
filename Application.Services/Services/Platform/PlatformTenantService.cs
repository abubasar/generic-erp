using Application.Core.Exceptions;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Entities.Platform;
using Application.Core.Interfaces;
using Application.Services.Services.Auth.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Platform
{
    public interface IPlatformTenantService
    {
        Task<IReadOnlyList<TenantListItem>> ListAsync(string? search, string? status);
        Task<TenantDetail?> GetAsync(Guid tenantId);
        Task<TenantDetail> CreateAsync(CreateTenantRequest request);
        Task<TenantDetail> SetStatusAsync(Guid tenantId, string status);
        Task<TenantDetail> SetSubscriptionAsync(Guid tenantId, SetSubscriptionRequest request);
        Task<TenantDetail> ToggleModuleAsync(Guid tenantId, ToggleModuleRequest request);
        Task<TenantDetail> SetQuotaAsync(Guid tenantId, SetQuotaRequest request);
        Task<ImpersonateResult> ImpersonateAsync(Guid tenantId);
    }

    public sealed class PlatformTenantService : IPlatformTenantService
    {
        private readonly DataContext _db;
        private readonly IPlatformAuditWriter _audit;
        private readonly IAuthService _authService;

        public PlatformTenantService(DataContext db, IPlatformAuditWriter audit, IAuthService authService)
        {
            _db = db;
            _audit = audit;
            _authService = authService;
        }

        public async Task<IReadOnlyList<TenantListItem>> ListAsync(string? search, string? status)
        {
            var q = _db.Set<Tenant>().IgnoreQueryFilters().Where(t => !t.Deleted);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(t => t.Name.Contains(s) || t.Code.Contains(s) || (t.Subdomain != null && t.Subdomain.Contains(s)));
            }
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(t => t.Status == status);

            var tenants = await q.OrderByDescending(t => t.CreatedOn).ToListAsync();
            var ids = tenants.Select(t => t.Id).ToList();

            var subs = await _db.Subscriptions.Where(s => ids.Contains(s.TenantId))
                .GroupBy(s => s.TenantId)
                .Select(g => g.OrderByDescending(s => s.PeriodEnd).First())
                .ToListAsync();
            var moduleCounts = await _db.TenantModules.Where(m => ids.Contains(m.TenantId) && m.Status == "Active")
                .GroupBy(m => m.TenantId).Select(g => new { TenantId = g.Key, Count = g.Count() }).ToListAsync();

            return tenants.Select(t =>
            {
                var sub = subs.FirstOrDefault(s => s.TenantId == t.Id);
                return new TenantListItem(t.Id, t.Code, t.Name, t.Subdomain, t.BusinessTemplateKey,
                    t.Status ?? "Unknown", sub?.Status, sub?.PlanKey,
                    moduleCounts.FirstOrDefault(m => m.TenantId == t.Id)?.Count ?? 0, t.CreatedOn);
            }).ToList();
        }

        public async Task<TenantDetail?> GetAsync(Guid tenantId)
        {
            var t = await _db.Set<Tenant>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == tenantId && !x.Deleted);
            if (t is null) return null;

            var sub = await _db.Subscriptions.Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.PeriodEnd).FirstOrDefaultAsync();
            var modules = await (from tm in _db.TenantModules
                                 join m in _db.Modules on tm.ModuleKey equals m.Key
                                 where tm.TenantId == tenantId
                                 orderby m.SortOrder
                                 select new TenantModuleInfo(tm.ModuleKey, m.Name, tm.Status, tm.ActivatedOn, tm.ExpiresOn))
                                .ToListAsync();
            var quotas = await _db.Entitlements.Where(e => e.TenantId == tenantId)
                .OrderBy(e => e.Key).Select(e => new QuotaInfo(e.Key, e.Limit)).ToListAsync();

            return new TenantDetail(t.Id, t.Code, t.Name, t.Subdomain, t.CustomDomain, t.BusinessTemplateKey,
                t.Status ?? "Unknown", t.Currency, t.BusinessType, t.DbConnectionKey, t.CreatedOn,
                sub is null ? null : new SubscriptionInfo(sub.Id, sub.PlanKey, sub.Status, sub.PeriodStart, sub.PeriodEnd, sub.TrialEndsOn),
                modules, quotas);
        }

        public async Task<TenantDetail> CreateAsync(CreateTenantRequest request)
        {
            var code = request.Code.Trim();
            var name = request.Name.Trim();
            if (await _db.Set<Tenant>().IgnoreQueryFilters().AnyAsync(t => t.Code == code && !t.Deleted))
                throw new BadRequestException($"A tenant with code '{code}' already exists.");

            var template = await _db.BusinessTemplates.FirstOrDefaultAsync(b => b.Key == request.BusinessTemplateKey)
                ?? throw new BadRequestException($"Unknown business template '{request.BusinessTemplateKey}'.");

            var subdomain = string.IsNullOrWhiteSpace(request.Subdomain) ? null : request.Subdomain.Trim().ToLowerInvariant();
            if (subdomain is not null && await _db.Set<Tenant>().IgnoreQueryFilters().AnyAsync(t => t.Subdomain == subdomain && !t.Deleted))
                throw new BadRequestException($"Sub-domain '{subdomain}' is taken.");

            Plan? plan = null;
            if (!string.IsNullOrWhiteSpace(request.PlanKey))
                plan = await _db.Plans.FirstOrDefaultAsync(p => p.Key == request.PlanKey)
                    ?? throw new BadRequestException($"Unknown plan '{request.PlanKey}'.");

            var now = DateTime.UtcNow;
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Email = request.Email,
                ContactNo = request.ContactNo,
                Address = request.Address,
                Subdomain = subdomain,
                BusinessTemplateKey = template.Key,
                BusinessType = template.Key switch { "pharmacy" => 1, "feed" => 2, _ => 0 },
                Status = "Active",
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BDT" : request.Currency.Trim().ToUpperInvariant(),
                TimeZoneId = "Asia/Dhaka",
                CreatedOn = now,
                UpdatedOn = now,
                Deleted = false,
            };
            _db.Set<Tenant>().Add(tenant);

            var moduleKeys = (plan?.ModuleKeys ?? template.DefaultModuleKeys)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct().ToList();
            var known = await _db.Modules.Select(m => m.Key).ToListAsync();
            foreach (var key in moduleKeys.Where(known.Contains))
            {
                _db.TenantModules.Add(new TenantModule
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id, ModuleKey = key,
                    Status = "Active", ActivatedOn = now,
                });
            }

            foreach (var (qKey, qLimit) in ParseQuotas(plan?.Quotas))
                _db.Entitlements.Add(new Entitlement { Id = Guid.NewGuid(), TenantId = tenant.Id, Key = qKey, Limit = qLimit });

            _db.Subscriptions.Add(new Subscription
            {
                Id = Guid.NewGuid(), TenantId = tenant.Id, PlanKey = plan?.Key,
                Status = "Active", PeriodStart = now, PeriodEnd = now.AddYears(1),
            });

            _audit.Add("tenant.create", tenant.Id, $"code={code}; template={template.Key}; plan={plan?.Key ?? "-"}; modules={string.Join('|', moduleKeys)}");
            await _db.SaveChangesAsync();

            return (await GetAsync(tenant.Id))!;
        }

        public async Task<TenantDetail> SetStatusAsync(Guid tenantId, string status)
        {
            var allowed = new[] { "Active", "Trial", "PastDue", "Suspended", "Cancelled" };
            if (!allowed.Contains(status)) throw new BadRequestException($"Invalid status '{status}'.");

            var t = await _db.Set<Tenant>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == tenantId && !x.Deleted)
                ?? throw new BadRequestException("Tenant not found.");
            var before = t.Status;
            t.Status = status;
            t.UpdatedOn = DateTime.UtcNow;

            var sub = await _db.Subscriptions.Where(s => s.TenantId == tenantId).OrderByDescending(s => s.PeriodEnd).FirstOrDefaultAsync();
            if (sub is not null) sub.Status = status;

            _audit.Add("tenant.status", tenantId, $"{before} -> {status}");
            await _db.SaveChangesAsync();
            return (await GetAsync(tenantId))!;
        }

        public async Task<TenantDetail> SetSubscriptionAsync(Guid tenantId, SetSubscriptionRequest request)
        {
            var t = await _db.Set<Tenant>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == tenantId && !x.Deleted)
                ?? throw new BadRequestException("Tenant not found.");

            if (request.PlanKey is not null && !await _db.Plans.AnyAsync(p => p.Key == request.PlanKey))
                throw new BadRequestException($"Unknown plan '{request.PlanKey}'.");

            var sub = await _db.Subscriptions.Where(s => s.TenantId == tenantId).OrderByDescending(s => s.PeriodEnd).FirstOrDefaultAsync();
            if (sub is null)
            {
                sub = new Subscription { Id = Guid.NewGuid(), TenantId = tenantId, PeriodStart = DateTime.UtcNow, PeriodEnd = DateTime.UtcNow.AddYears(1) };
                _db.Subscriptions.Add(sub);
            }
            sub.Status = request.Status;
            sub.PlanKey = request.PlanKey;
            if (request.PeriodEnd is not null) sub.PeriodEnd = request.PeriodEnd.Value;
            sub.TrialEndsOn = request.TrialEndsOn;
            t.Status = request.Status;
            t.UpdatedOn = DateTime.UtcNow;

            _audit.Add("subscription.set", tenantId, $"status={request.Status}; plan={request.PlanKey ?? "-"}");
            await _db.SaveChangesAsync();
            return (await GetAsync(tenantId))!;
        }

        public async Task<TenantDetail> ToggleModuleAsync(Guid tenantId, ToggleModuleRequest request)
        {
            if (!await _db.Set<Tenant>().IgnoreQueryFilters().AnyAsync(x => x.Id == tenantId && !x.Deleted))
                throw new BadRequestException("Tenant not found.");
            var module = await _db.Modules.FirstOrDefaultAsync(m => m.Key == request.ModuleKey)
                ?? throw new BadRequestException($"Unknown module '{request.ModuleKey}'.");
            if (module.Category == "Core" && !request.Enabled)
                throw new BadRequestException($"'{module.Key}' is a core module and cannot be disabled.");

            var tm = await _db.TenantModules.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ModuleKey == request.ModuleKey);
            if (request.Enabled)
            {
                if (tm is null)
                    _db.TenantModules.Add(new TenantModule { Id = Guid.NewGuid(), TenantId = tenantId, ModuleKey = request.ModuleKey, Status = "Active", ActivatedOn = DateTime.UtcNow });
                else
                    tm.Status = "Active";
            }
            else if (tm is not null)
            {
                tm.Status = "Suspended";
            }

            _audit.Add("module.toggle", tenantId, $"{request.ModuleKey}={(request.Enabled ? "on" : "off")}");
            await _db.SaveChangesAsync();
            return (await GetAsync(tenantId))!;
        }

        public async Task<TenantDetail> SetQuotaAsync(Guid tenantId, SetQuotaRequest request)
        {
            if (!await _db.Set<Tenant>().IgnoreQueryFilters().AnyAsync(x => x.Id == tenantId && !x.Deleted))
                throw new BadRequestException("Tenant not found.");

            var ent = await _db.Entitlements.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Key == request.Key);
            if (request.Limit <= 0)
            {
                if (ent is not null) _db.Entitlements.Remove(ent);
            }
            else if (ent is null)
            {
                _db.Entitlements.Add(new Entitlement { Id = Guid.NewGuid(), TenantId = tenantId, Key = request.Key, Limit = request.Limit });
            }
            else
            {
                ent.Limit = request.Limit;
            }

            _audit.Add("quota.set", tenantId, $"{request.Key}={request.Limit}");
            await _db.SaveChangesAsync();
            return (await GetAsync(tenantId))!;
        }

        public async Task<ImpersonateResult> ImpersonateAsync(Guid tenantId)
        {
            var t = await _db.Set<Tenant>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == tenantId && !x.Deleted)
                ?? throw new BadRequestException("Tenant not found.");

            // Pick a sign-in identity: an active user, preferring an admin-ish role name.
            var user = await _db.Set<User>().IgnoreQueryFilters()
                .Where(u => u.TenantId == tenantId && !u.Deleted)
                .Join(_db.Set<Role>().IgnoreQueryFilters(), u => u.RoleId, r => r.Id, (u, r) => new { u, r.Name })
                .OrderByDescending(x => x.Name == "SA" || x.Name == "Owner" || x.Name.Contains("Admin"))
                .Select(x => x.u)
                .FirstOrDefaultAsync()
                ?? throw new BadRequestException("This tenant has no user to act as.");

            var (accessToken, refreshToken) = await _authService.IssueTokensForUserAsync(user.Id);

            _audit.Add("tenant.impersonate", tenantId, $"actingAs userId={user.Id} ({user.Username})");
            await _db.SaveChangesAsync();

            return new ImpersonateResult(accessToken, refreshToken, tenantId, t.Name, user.Username);
        }

        private static IEnumerable<(string key, int limit)> ParseQuotas(string? quotas)
        {
            if (string.IsNullOrWhiteSpace(quotas)) yield break;
            foreach (var pair in quotas.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2 && int.TryParse(kv[1].Trim(), out var limit) && limit > 0)
                    yield return (kv[0].Trim(), limit);
            }
        }
    }
}
