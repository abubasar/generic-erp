using Application.Core.Data;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Platform
{
    public interface IPlatformUsageService
    {
        Task<PlatformUsageSummary> GetSummaryAsync();
        Task<IReadOnlyList<AuditEntryDto>> GetAuditAsync(Guid? tenantId, int take);
    }

    public sealed class PlatformUsageService : IPlatformUsageService
    {
        private readonly DataContext _db;

        public PlatformUsageService(DataContext db) => _db = db;

        public async Task<PlatformUsageSummary> GetSummaryAsync()
        {
            var tenants = await _db.Set<Tenant>().IgnoreQueryFilters().Where(t => !t.Deleted)
                .Select(t => new { t.Status }).ToListAsync();

            int Count(string s) => tenants.Count(t => string.Equals(t.Status, s, StringComparison.OrdinalIgnoreCase));

            var adoption = await (from tm in _db.TenantModules
                                  join m in _db.Modules on tm.ModuleKey equals m.Key
                                  where tm.Status == "Active"
                                  group m by new { m.Key, m.Name } into g
                                  orderby g.Count() descending
                                  select new ModuleAdoption(g.Key.Key, g.Key.Name, g.Count()))
                                 .ToListAsync();

            var (mrr, currency) = await EstimateMrrAsync();

            var recent = await _db.PlatformAuditLogs.OrderByDescending(a => a.CreatedOn).Take(15)
                .Select(a => new AuditEntryDto(a.Id, a.PlatformAdminEmail, a.Action, a.TenantId, a.Detail, a.CreatedOn))
                .ToListAsync();

            return new PlatformUsageSummary(
                tenants.Count, Count("Active"), Count("Trial"), Count("Suspended"),
                mrr, currency, adoption, recent);
        }

        public async Task<IReadOnlyList<AuditEntryDto>> GetAuditAsync(Guid? tenantId, int take)
        {
            var q = _db.PlatformAuditLogs.AsQueryable();
            if (tenantId is not null) q = q.Where(a => a.TenantId == tenantId);
            return await q.OrderByDescending(a => a.CreatedOn).Take(Math.Clamp(take, 1, 500))
                .Select(a => new AuditEntryDto(a.Id, a.PlatformAdminEmail, a.Action, a.TenantId, a.Detail, a.CreatedOn))
                .ToListAsync();
        }

        /// <summary>
        /// Rough recurring revenue: for every non-suspended subscription with a plan,
        /// the plan's monthly price from the live price book. Add-on modules ignored
        /// for now.
        /// </summary>
        private async Task<(decimal mrr, string currency)> EstimateMrrAsync()
        {
            var live = await _db.PriceBooks.Include(b => b.Entries)
                .Where(b => b.Status == "Published").OrderByDescending(b => b.Version).FirstOrDefaultAsync();
            if (live is null) return (0m, "BDT");
            var currency = live.Currency;

            var planPrice = live.Entries.Where(e => e.ItemType == "plan")
                .ToDictionary(e => e.ItemKey, e => e.MonthlyPrice);

            var activePlans = await _db.Subscriptions
                .Where(s => s.PlanKey != null && s.Status != "Suspended" && s.Status != "Cancelled")
                .Select(s => s.PlanKey!)
                .ToListAsync();

            return (activePlans.Sum(pk => planPrice.TryGetValue(pk.ToLowerInvariant(), out var p) ? p : 0m), currency);
        }
    }
}
