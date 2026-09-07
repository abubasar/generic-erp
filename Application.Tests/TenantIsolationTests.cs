using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

/// <summary>
/// Phase 0: proves the automatic EF Core tenant query filter isolates tenants.
/// Uses the InMemory provider — the filter lives in the shared query pipeline,
/// so provider choice does not matter for what is being tested here. Each
/// <see cref="DataContext"/> is bound to one tenant at construction, exactly as a
/// per-request context is in the app.
/// </summary>
public sealed class TenantIsolationTests
{
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly string _dbName = $"tenant-isolation-{Guid.NewGuid()}";

    public TenantIsolationTests()
    {
        using var seed = Context(Guid.Empty);
        seed.Categories.AddRange(
            Category(TenantA, "A-Feed", deleted: false),
            Category(TenantA, "A-Premix", deleted: false),
            Category(TenantA, "A-Archived", deleted: true),
            Category(TenantB, "B-Tablets", deleted: false));
        seed.SaveChanges();
    }

    [Fact]
    public void Query_returns_only_the_current_tenants_rows()
    {
        using (var a = Context(TenantA))
            Assert.Equal(new[] { "A-Feed", "A-Premix" }, Names(a));

        using (var b = Context(TenantB))
            Assert.Equal(new[] { "B-Tablets" }, Names(b));
    }

    [Fact]
    public void No_tenant_returns_nothing()
    {
        using var none = Context(Guid.Empty);
        Assert.Empty(none.Categories.ToList());
    }

    [Fact]
    public void Soft_deleted_rows_stay_hidden_for_the_owning_tenant()
    {
        using var a = Context(TenantA);
        Assert.DoesNotContain("A-Archived", Names(a));
    }

    [Fact]
    public void IgnoreQueryFilters_bypasses_tenant_and_soft_delete()
    {
        using var a = Context(TenantA);
        var all = a.Categories.IgnoreQueryFilters().Select(c => c.Name).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "A-Archived", "A-Feed", "A-Premix", "B-Tablets" }, all);
    }

    [Fact]
    public void Lookup_by_id_cannot_reach_another_tenant()
    {
        Guid bId;
        using (var seed = Context(Guid.Empty))
        {
            var c = Category(TenantB, "B-Only", deleted: false);
            seed.Categories.Add(c);
            seed.SaveChanges();
            bId = c.Id;
        }

        using (var a = Context(TenantA))
            Assert.Null(a.Categories.SingleOrDefault(c => c.Id == bId));

        using (var b = Context(TenantB))
            Assert.NotNull(b.Categories.SingleOrDefault(c => c.Id == bId));
    }

    private DataContext Context(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        return new DataContext(options, new FakeTenantContext(tenantId));
    }

    private static List<string> Names(DataContext db) =>
        db.Categories.Select(c => c.Name).OrderBy(n => n).ToList();

    private static Category Category(Guid tenantId, string name, bool deleted) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        TenantId = tenantId,
        Deleted = deleted,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow,
    };

    private sealed class FakeTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid TenantId { get; } = tenantId;
        public bool HasTenant => TenantId != Guid.Empty;
    }
}
