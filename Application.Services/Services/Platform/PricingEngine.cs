using Application.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Platform
{
    public interface IPricingEngine
    {
        Task<PricingQuote> QuoteAsync(PricingRequest request);
    }

    public sealed record PricingRequest(
        string? PlanKey,
        IReadOnlyList<string>? ModuleKeys,
        int Users,
        int Branches,
        int PosTerminals);

    public sealed record PricingLine(string Label, string Detail, decimal Amount);

    public sealed record PricingQuote(
        string Currency,
        int? PriceBookVersion,
        IReadOnlyList<PricingLine> Lines,
        decimal MonthlyTotal,
        bool PricingConfigured);

    /// <summary>
    /// Turns a plan / module selection + team sizes into an itemised monthly
    /// quote against the live <see cref="Application.Core.Entities.Platform.PriceBook"/>.
    /// Entry item types used: "base" (one), "plan:&lt;key&gt;", "module:&lt;key&gt;",
    /// "unit:&lt;quotaKey&gt;" (UnitPrice = per extra). "Included" counts come from
    /// the chosen plan's Quotas; build-your-own includes nothing.
    /// See <c>docs/saas-platform-plan.md</c> §06.
    /// </summary>
    public sealed class PricingEngine : IPricingEngine
    {
        private readonly DataContext _db;

        public PricingEngine(DataContext db) => _db = db;

        private static readonly (string quotaKey, string label)[] Meters =
        {
            ("max_users", "Users"),
            ("max_branches", "Branches"),
            ("max_pos_terminals", "POS terminals"),
        };

        public async Task<PricingQuote> QuoteAsync(PricingRequest request)
        {
            var book = await _db.PriceBooks.Include(b => b.Entries)
                .Where(b => b.Status == "Published").OrderByDescending(b => b.Version).FirstOrDefaultAsync();

            if (book is null)
                return new PricingQuote("BDT", null, Array.Empty<PricingLine>(), 0m, PricingConfigured: false);

            decimal Price(string type, string key) =>
                book.Entries.FirstOrDefault(e => e.ItemType == type && e.ItemKey == key)?.MonthlyPrice ?? 0m;
            decimal Unit(string key) =>
                book.Entries.FirstOrDefault(e => e.ItemType == "unit" && e.ItemKey == key)?.UnitPrice ?? 0m;

            var lines = new List<PricingLine>();

            // Base
            var basePrice = Price("base", "base");
            if (basePrice > 0) lines.Add(new PricingLine("Base platform", "always included", basePrice));

            // Plan or à la carte modules
            var includedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var quotas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(request.PlanKey))
            {
                var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Key == request.PlanKey!.ToLowerInvariant());
                if (plan is not null)
                {
                    foreach (var k in Split(plan.ModuleKeys)) includedModules.Add(k);
                    foreach (var (k, v) in ParseQuotas(plan.Quotas)) quotas[k] = v;
                    lines.Add(new PricingLine($"{plan.Name} plan", string.Join(", ", includedModules), Price("plan", plan.Key)));
                }
            }

            var selected = (request.ModuleKeys ?? Array.Empty<string>())
                .Select(m => m.Trim().ToLowerInvariant()).Where(m => m.Length > 0).ToHashSet();
            foreach (var key in selected.Where(k => !includedModules.Contains(k)).OrderBy(k => k))
            {
                var add = Price("module", key);
                lines.Add(new PricingLine(TitleCase(key), add > 0 ? "add-on" : "included", add));
            }

            // Team & locations
            foreach (var (quotaKey, label) in Meters)
            {
                var requested = quotaKey switch
                {
                    "max_users" => request.Users,
                    "max_branches" => request.Branches,
                    _ => request.PosTerminals,
                };
                if (requested <= 0) continue;
                var included = quotas.TryGetValue(quotaKey, out var inc) ? inc : 0;
                var extra = Math.Max(0, requested - included);
                var unit = Unit(quotaKey);
                var amount = extra * unit;
                lines.Add(new PricingLine(label,
                    $"{requested} ({included} included{(extra > 0 ? $", +{extra} × {unit:0.##}" : "")})",
                    amount));
            }

            var total = lines.Sum(l => l.Amount);
            return new PricingQuote(book.Currency, book.Version, lines, total, PricingConfigured: true);
        }

        private static IEnumerable<string> Split(string? csv) =>
            (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant());

        private static IEnumerable<(string key, int limit)> ParseQuotas(string? quotas)
        {
            foreach (var pair in Split(quotas))
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2 && int.TryParse(kv[1].Trim(), out var n)) yield return (kv[0].Trim(), n);
            }
        }

        private static string TitleCase(string key) =>
            string.Join(' ', key.Split('-', '_').Select(w => w.Length == 0 ? w : char.ToUpper(w[0]) + w[1..]));
    }
}
