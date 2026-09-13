using Application.Core.Entities.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Core.Data.Configurations.Platform;

public class PlatformAdminConfiguration : IEntityTypeConfiguration<PlatformAdmin>
{
    public void Configure(EntityTypeBuilder<PlatformAdmin> e)
    {
        e.ToTable("PlatformAdmin");
        e.HasKey(x => x.Id);
        e.Property(x => x.Email).IsRequired().HasMaxLength(200);
        e.Property(x => x.Name).IsRequired().HasMaxLength(150);
        e.Property(x => x.Role).IsRequired().HasMaxLength(20);
        e.HasIndex(x => x.Email).IsUnique();
    }
}

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> e)
    {
        e.ToTable("Plan");
        e.HasKey(x => x.Key);
        e.Property(x => x.Key).HasMaxLength(50);
        e.Property(x => x.Name).IsRequired().HasMaxLength(100);
        e.Property(x => x.Description).HasMaxLength(500);
        e.Property(x => x.ModuleKeys).IsRequired().HasMaxLength(1000);
        e.Property(x => x.Quotas).IsRequired().HasMaxLength(1000);

        // Predefined plans (docs/saas-platform-plan.md §06). All three grant every
        // module — Pharma/Feed both need all 7 today and modules aren't a real
        // per-plan lever yet (see pharma-feed-share-all-features) — plans differ by
        // team-size quotas only, which the seeded PriceBook below prices per extra unit.
        const string all = "configuration,inventory,purchase,sales,production,accounts,report";
        e.HasData(
            new Plan { Key = "starter", Name = "Starter", Description = "For a small shop just getting started.", ModuleKeys = all, Quotas = "max_users=2,max_branches=1,max_pos_terminals=0", SortOrder = 1 },
            new Plan { Key = "business", Name = "Business", Description = "For a growing team with more than one location.", ModuleKeys = all, Quotas = "max_users=8,max_branches=3,max_pos_terminals=2", SortOrder = 2 },
            new Plan { Key = "enterprise", Name = "Enterprise", Description = "For a large business or group of companies.", ModuleKeys = all, Quotas = "max_users=50,max_branches=20,max_pos_terminals=10", SortOrder = 3 });
    }
}

public class PriceBookConfiguration : IEntityTypeConfiguration<PriceBook>
{
    public static readonly Guid SeedPriceBookId = new("11111111-1111-1111-1111-111111111111");

    public void Configure(EntityTypeBuilder<PriceBook> e)
    {
        e.ToTable("PriceBook");
        e.HasKey(x => x.Id);
        e.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        e.Property(x => x.Status).IsRequired().HasMaxLength(20);
        e.Property(x => x.Note).HasMaxLength(500);
        e.HasIndex(x => x.Version).IsUnique();
        e.HasMany(x => x.Entries).WithOne(x => x.PriceBook).HasForeignKey(x => x.PriceBookId).OnDelete(DeleteBehavior.Cascade);

        // Published so PricingEngine (and the anonymous signup quote) has real
        // numbers from the moment the plans above exist. A platform admin can
        // always draft + publish a new version later — see PlatformCatalogController.
        e.HasData(new PriceBook
        {
            Id = SeedPriceBookId,
            Version = 1,
            Currency = "BDT",
            Status = "Published",
            PublishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Note = "Initial seed price book.",
            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        });
    }
}

public class PriceBookEntryConfiguration : IEntityTypeConfiguration<PriceBookEntry>
{
    public void Configure(EntityTypeBuilder<PriceBookEntry> e)
    {
        e.ToTable("PriceBookEntry");
        e.HasKey(x => x.Id);
        e.Property(x => x.ItemType).IsRequired().HasMaxLength(20);
        e.Property(x => x.ItemKey).IsRequired().HasMaxLength(50);
        e.Property(x => x.MonthlyPrice).HasColumnType("decimal(18,2)");
        e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        e.HasIndex(x => new { x.PriceBookId, x.ItemType, x.ItemKey }).IsUnique();

        var bookId = PriceBookConfiguration.SeedPriceBookId;
        e.HasData(
            new PriceBookEntry { Id = new Guid("11111111-1111-1111-1111-111111111101"), PriceBookId = bookId, ItemType = "plan", ItemKey = "starter", MonthlyPrice = 1500m },
            new PriceBookEntry { Id = new Guid("11111111-1111-1111-1111-111111111102"), PriceBookId = bookId, ItemType = "plan", ItemKey = "business", MonthlyPrice = 3500m },
            new PriceBookEntry { Id = new Guid("11111111-1111-1111-1111-111111111103"), PriceBookId = bookId, ItemType = "plan", ItemKey = "enterprise", MonthlyPrice = 8000m },
            new PriceBookEntry { Id = new Guid("11111111-1111-1111-1111-111111111111"), PriceBookId = bookId, ItemType = "unit", ItemKey = "max_users", MonthlyPrice = 0m, UnitPrice = 300m },
            new PriceBookEntry { Id = new Guid("11111111-1111-1111-1111-111111111112"), PriceBookId = bookId, ItemType = "unit", ItemKey = "max_branches", MonthlyPrice = 0m, UnitPrice = 500m },
            new PriceBookEntry { Id = new Guid("11111111-1111-1111-1111-111111111113"), PriceBookId = bookId, ItemType = "unit", ItemKey = "max_pos_terminals", MonthlyPrice = 0m, UnitPrice = 350m });
    }
}

public class ProvisioningStepConfiguration : IEntityTypeConfiguration<ProvisioningStep>
{
    public void Configure(EntityTypeBuilder<ProvisioningStep> e)
    {
        e.ToTable("ProvisioningStep");
        e.HasKey(x => x.Id);
        e.Property(x => x.StepKey).IsRequired().HasMaxLength(50);
        e.Property(x => x.Status).IsRequired().HasMaxLength(20);
        e.Property(x => x.Error).HasMaxLength(2000);
        e.HasIndex(x => new { x.TenantId, x.StepKey }).IsUnique();
    }
}

public class PlatformAuditLogConfiguration : IEntityTypeConfiguration<PlatformAuditLog>
{
    public void Configure(EntityTypeBuilder<PlatformAuditLog> e)
    {
        e.ToTable("PlatformAuditLog");
        e.HasKey(x => x.Id);
        e.Property(x => x.PlatformAdminEmail).IsRequired().HasMaxLength(200);
        e.Property(x => x.Action).IsRequired().HasMaxLength(50);
        e.Property(x => x.Detail).HasMaxLength(4000);
        e.HasIndex(x => x.CreatedOn);
        e.HasIndex(x => x.TenantId);
    }
}

public class PlatformInvoiceConfiguration : IEntityTypeConfiguration<PlatformInvoice>
{
    public void Configure(EntityTypeBuilder<PlatformInvoice> e)
    {
        e.ToTable("PlatformInvoice");
        e.HasKey(x => x.Id);
        e.Property(x => x.Number).IsRequired().HasMaxLength(30);
        e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        e.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        e.Property(x => x.Status).IsRequired().HasMaxLength(20);
        e.Property(x => x.Note).HasMaxLength(500);
        e.HasIndex(x => x.Number).IsUnique();
        e.HasIndex(x => new { x.TenantId, x.IssuedOn });
        e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
