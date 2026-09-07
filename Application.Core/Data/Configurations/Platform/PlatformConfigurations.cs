using Application.Core.Entities;
using Application.Core.Entities.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Core.Data.Configurations.Platform;

public class ModuleConfiguration : IEntityTypeConfiguration<PlatformModule>
{
    public void Configure(EntityTypeBuilder<PlatformModule> e)
    {
        e.ToTable("Module");
        e.HasKey(x => x.Key);
        e.Property(x => x.Key).HasMaxLength(50);
        e.Property(x => x.Name).IsRequired().HasMaxLength(100);
        e.Property(x => x.Category).IsRequired().HasMaxLength(20);
        e.Property(x => x.Description).HasMaxLength(500);
        e.Property(x => x.DependsOn).HasMaxLength(500);
        e.Property(x => x.PermissionGroup).HasMaxLength(100);

        // Seeded from Application.Core/Constants/Permissions.cs -> AccessModules.
        // Pharma and Feed both get all of these (industry = behaviour profile, not entitlement).
        e.HasData(
            new PlatformModule { Key = "configuration", Name = "Configuration & masters", Category = "Core", PermissionGroup = "Permissions.AccessModules.Configuration", SortOrder = 1, Description = "Company, stores, products, customers, suppliers, settings." },
            new PlatformModule { Key = "inventory", Name = "Inventory & stock", Category = "Business", PermissionGroup = "Permissions.AccessModules.Inventory", SortOrder = 2, Description = "Stock ledger, adjustments, transfers." },
            new PlatformModule { Key = "purchase", Name = "Purchase", Category = "Business", PermissionGroup = "Permissions.AccessModules.Purchase", DependsOn = "inventory", SortOrder = 3, Description = "Requisition, PO, GRN, purchase invoice, returns, LC." },
            new PlatformModule { Key = "sales", Name = "Sales", Category = "Business", PermissionGroup = "Permissions.AccessModules.Sales", DependsOn = "inventory", SortOrder = 4, Description = "Quotation, sale order, delivery note, invoice, returns." },
            new PlatformModule { Key = "production", Name = "Production", Category = "Business", PermissionGroup = "Permissions.AccessModules.Production", DependsOn = "inventory", SortOrder = 5, Description = "BOM / formula, manufacturing orders, raw material." },
            new PlatformModule { Key = "accounts", Name = "Accounting", Category = "Business", PermissionGroup = "Permissions.AccessModules.Accounts", SortOrder = 6, Description = "Chart of accounts, journals, vouchers, fund transfers." },
            new PlatformModule { Key = "report", Name = "Reports & analytics", Category = "Business", PermissionGroup = "Permissions.AccessModules.Report", SortOrder = 7, Description = "COGS, ledgers, stock and sales analysis." });
    }
}

public class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
{
    public void Configure(EntityTypeBuilder<TenantModule> e)
    {
        e.ToTable("TenantModule");
        e.HasKey(x => x.Id);
        e.Property(x => x.ModuleKey).IsRequired().HasMaxLength(50);
        e.Property(x => x.Status).IsRequired().HasMaxLength(20);
        e.HasIndex(x => new { x.TenantId, x.ModuleKey }).IsUnique();
        e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.Module).WithMany().HasForeignKey(x => x.ModuleKey).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BusinessTemplateConfiguration : IEntityTypeConfiguration<BusinessTemplate>
{
    public void Configure(EntityTypeBuilder<BusinessTemplate> e)
    {
        e.ToTable("BusinessTemplate");
        e.HasKey(x => x.Key);
        e.Property(x => x.Key).HasMaxLength(50);
        e.Property(x => x.Name).IsRequired().HasMaxLength(100);
        e.Property(x => x.Description).HasMaxLength(500);
        e.Property(x => x.DefaultModuleKeys).IsRequired().HasMaxLength(1000);
        e.Property(x => x.IndustryProfileKey).IsRequired().HasMaxLength(50);

        const string all = "configuration,inventory,purchase,sales,production,accounts,report";
        e.HasData(
            new BusinessTemplate { Key = "pharmacy", Name = "Pharmaceutical", IndustryProfileKey = "pharmacy", DefaultModuleKeys = all, SortOrder = 1, Description = "Pharmaceutical manufacturing & distribution." },
            new BusinessTemplate { Key = "feed", Name = "Feed industry", IndustryProfileKey = "feed", DefaultModuleKeys = all, SortOrder = 2, Description = "Animal feed / poultry feed manufacturing & distribution." });
    }
}

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> e)
    {
        e.ToTable("Subscription");
        e.HasKey(x => x.Id);
        e.Property(x => x.PlanKey).HasMaxLength(50);
        e.Property(x => x.Status).IsRequired().HasMaxLength(20);
        e.HasIndex(x => x.TenantId);
        e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EntitlementConfiguration : IEntityTypeConfiguration<Entitlement>
{
    public void Configure(EntityTypeBuilder<Entitlement> e)
    {
        e.ToTable("Entitlement");
        e.HasKey(x => x.Id);
        e.Property(x => x.Key).IsRequired().HasMaxLength(50);
        e.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> e)
    {
        e.ToTable("TenantSetting");
        e.HasKey(x => x.Id);
        e.Property(x => x.Key).IsRequired().HasMaxLength(100);
        e.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
