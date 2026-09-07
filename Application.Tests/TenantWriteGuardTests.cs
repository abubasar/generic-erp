using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

/// <summary>
/// Phase 0: the UnitOfWork save guard fails closed on cross-tenant writes, and
/// BaseRepository.FindAsync (which bypasses query filters) cannot reach another
/// tenant's row.
/// </summary>
public sealed class TenantWriteGuardTests : IDisposable
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly string _dbName = $"write-guard-{Guid.NewGuid()}";
    private readonly Guid _bCategoryId = Guid.NewGuid();

    public TenantWriteGuardTests()
    {
        TenantScope.CurrentTenantId = Guid.Empty;
        using var seed = NewContext(Guid.Empty);
        seed.Categories.Add(new Category
        {
            Id = _bCategoryId,
            Name = "B-Category",
            TenantId = TenantB,
            Deleted = false,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        });
        seed.SaveChanges();
    }

    [Fact]
    public async Task Modifying_another_tenants_row_is_blocked_on_save()
    {
        TenantScope.CurrentTenantId = TenantA;
        var db = NewContext(TenantA);
        var uow = new UnitOfWork(db, HttpContextWithUser());

        // Reach past the read filter the way a bug would (unfiltered read, then edit).
        var bRow = db.Categories.IgnoreQueryFilters().Single(c => c.Id == _bCategoryId);
        bRow.Name = "hijacked";

        await Assert.ThrowsAsync<UnauthorizationException>(() => uow.SaveChangesAsync());
    }

    [Fact]
    public async Task Deleting_another_tenants_row_is_blocked_on_save()
    {
        TenantScope.CurrentTenantId = TenantA;
        var db = NewContext(TenantA);
        var uow = new UnitOfWork(db, HttpContextWithUser());

        var bRow = db.Categories.IgnoreQueryFilters().Single(c => c.Id == _bCategoryId);
        db.Categories.Remove(bRow);

        await Assert.ThrowsAsync<UnauthorizationException>(() => uow.SaveChangesAsync());
    }

    [Fact]
    public async Task Modifying_your_own_row_still_works()
    {
        TenantScope.CurrentTenantId = TenantA;
        var db = NewContext(TenantA);
        var uow = new UnitOfWork(db, HttpContextWithUser());

        var mine = new Category { Id = Guid.NewGuid(), Name = "A-Category", TenantId = TenantA, CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow };
        db.Categories.Add(mine);
        await uow.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var reload = db.Categories.Single(c => c.Id == mine.Id);
        reload.Name = "A-Category-renamed";
        await uow.SaveChangesAsync();

        db.ChangeTracker.Clear();
        Assert.Equal("A-Category-renamed", db.Categories.Single(c => c.Id == mine.Id).Name);
    }

    [Fact]
    public async Task FindAsync_by_id_does_not_return_another_tenants_row()
    {
        TenantScope.CurrentTenantId = TenantA;
        var db = NewContext(TenantA);
        var repo = new BaseRepository<Category>(db);

        await Assert.ThrowsAsync<NotFoundResultException>(() => repo.FindAsync(_bCategoryId));
    }

    private DataContext NewContext(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<DataContext>().UseInMemoryDatabase(_dbName).Options;
        return new DataContext(options, new NullTenantContext(tenantId));
    }

    private static IHttpContextAccessor HttpContextWithUser()
    {
        var ctx = new DefaultHttpContext();
        ctx.Items["UserName"] = "tester";
        return new HttpContextAccessor { HttpContext = ctx };
    }

    public void Dispose() => TenantScope.CurrentTenantId = Guid.Empty;
}
