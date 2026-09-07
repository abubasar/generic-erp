using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Tests;

/// <summary>
/// Guards the constructor-selection assumption: with both the scaffolded
/// single-arg constructor and the tenant-aware overload present, the DI container
/// must pick the tenant-aware one (this is how <c>AddDbContext</c> activates the
/// context in the app). If a re-scaffold or an EF change breaks this, this test
/// fails instead of tenants silently sharing data.
/// </summary>
public sealed class DataContextConstructionTests
{
    [Fact]
    public void AddDbContext_resolves_the_tenant_aware_constructor()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITenantContext>(_ => new NullTenantContext(Guid.Parse("33333333-3333-3333-3333-333333333333")));
        services.AddDbContext<DataContext>(o => o.UseInMemoryDatabase("ctor-check"));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();

        // The tenant-aware ctor stores the id in a private field the filter reads;
        // an empty filter value would mean the single-arg ctor was chosen.
        db.Categories.Add(new() { Id = Guid.NewGuid(), Name = "x", TenantId = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow });
        db.SaveChanges();
        db.ChangeTracker.Clear();

        Assert.Single(db.Categories.ToList());
    }
}
