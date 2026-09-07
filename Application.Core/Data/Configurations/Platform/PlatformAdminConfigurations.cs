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
    }
}

public class PriceBookConfiguration : IEntityTypeConfiguration<PriceBook>
{
    public void Configure(EntityTypeBuilder<PriceBook> e)
    {
        e.ToTable("PriceBook");
        e.HasKey(x => x.Id);
        e.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        e.Property(x => x.Status).IsRequired().HasMaxLength(20);
        e.Property(x => x.Note).HasMaxLength(500);
        e.HasIndex(x => x.Version).IsUnique();
        e.HasMany(x => x.Entries).WithOne(x => x.PriceBook).HasForeignKey(x => x.PriceBookId).OnDelete(DeleteBehavior.Cascade);
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
