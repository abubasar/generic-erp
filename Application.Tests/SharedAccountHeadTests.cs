using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

/// <summary>
/// The standard chart-of-accounts skeleton is stored once with
/// <see cref="TenancyConstants.SystemTenantId"/> and must resolve in every
/// tenant's context (the accounting engine keys off fixed account-head GUIDs),
/// while a real tenant's own accounts stay isolated.
/// </summary>
public sealed class SharedAccountHeadTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly string _dbName = $"shared-heads-{Guid.NewGuid()}";
    private readonly Guid _sharedHeadId = Guid.NewGuid();
    private readonly Guid _tenantAAccountId = Guid.NewGuid();

    public SharedAccountHeadTests()
    {
        using var seed = Context(Guid.Empty);
        seed.Set<Account>().AddRange(
            Account(_sharedHeadId, TenancyConstants.SystemTenantId, "Trade Receivable", "1-02-01"),
            Account(_tenantAAccountId, TenantA, "Customer X", "1-02-01-0001"),
            Account(Guid.NewGuid(), TenantB, "Customer Y", "1-02-01-0009"));
        seed.SaveChanges();
    }

    [Fact]
    public void Shared_head_is_visible_from_every_tenant()
    {
        using (var a = Context(TenantA))
            Assert.NotNull(a.Set<Account>().SingleOrDefault(x => x.Id == _sharedHeadId));
        using (var b = Context(TenantB))
            Assert.NotNull(b.Set<Account>().SingleOrDefault(x => x.Id == _sharedHeadId));
    }

    [Fact]
    public void Tenant_A_account_is_not_visible_to_tenant_B()
    {
        using var b = Context(TenantB);
        Assert.Null(b.Set<Account>().SingleOrDefault(x => x.Id == _tenantAAccountId));
    }

    [Fact]
    public void Each_tenant_sees_its_own_accounts_plus_the_shared_head_only()
    {
        using var a = Context(TenantA);
        Assert.Equal(new[] { "Customer X", "Trade Receivable" },
            a.Set<Account>().Select(x => x.Name).OrderBy(n => n).ToArray());
    }

    private DataContext Context(Guid tenantId) =>
        new(new DbContextOptionsBuilder<DataContext>().UseInMemoryDatabase(_dbName).Options,
            new NullTenantContext(tenantId));

    private static Account Account(Guid id, Guid tenantId, string name, string code) => new()
    {
        Id = id,
        TenantId = tenantId,
        Name = name,
        Code = code,
        AccountTypeId = Guid.NewGuid(),
        Deleted = false,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow,
    };
}
