using System.Linq.Expressions;
using System.Reflection;
using Application.Core.Common;
using Application.Core.Data.Configurations.Platform;
using Application.Core.Entities.Platform;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.StoredProcedureResult;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Data
{
    /// <summary>
    /// Hand-authored partial of the scaffolded <see cref="DataContext"/>. Everything
    /// here must live outside <c>Data/DataContext.cs</c> so an EF Power Tools
    /// re-scaffold (which regenerates that file) cannot wipe it:
    ///  - the tenant-aware constructor (the scaffold keeps its single-arg one; runtime
    ///    DI picks this greedier one because <see cref="ITenantContext"/> is registered);
    ///  - the global soft-delete filter (<c>ApplyGlobalFilter</c>) — previously a
    ///    hand-added line in the generated file;
    ///  - the per-<see cref="ITenantScoped"/> tenant + soft-delete query filter;
    ///  - the keyless SP-result mapping.
    /// See docs/saas-platform-plan.md §11 and docs/phase0-tenancy.md.
    /// </summary>
    public partial class DataContext
    {
        // Read by every tenant query filter. EF Core sees this as a member access on
        // the context instance and parameterises it per query (it is not baked into
        // the cached model). Guid.Empty => tenant-scoped queries return nothing.
        private readonly Guid _currentTenantId;

        public DataContext(DbContextOptions<DataContext> options, ITenantContext tenantContext)
            : base(options)
            => _currentTenantId = tenantContext?.TenantId ?? Guid.Empty;

        private static readonly FieldInfo CurrentTenantIdField =
            typeof(DataContext).GetField(nameof(_currentTenantId),
                BindingFlags.NonPublic | BindingFlags.Instance)!;

        // ---- Platform layer (Phase 1). Not ITenantScoped: the platform admin needs
        //      cross-tenant access; a tenant reaches its own rows via ITenantContext. ----
        public virtual DbSet<PlatformModule> Modules => Set<PlatformModule>();
        public virtual DbSet<TenantModule> TenantModules => Set<TenantModule>();
        public virtual DbSet<BusinessTemplate> BusinessTemplates => Set<BusinessTemplate>();
        public virtual DbSet<Subscription> Subscriptions => Set<Subscription>();
        public virtual DbSet<Entitlement> Entitlements => Set<Entitlement>();
        public virtual DbSet<TenantSetting> TenantSettings => Set<TenantSetting>();
        public virtual DbSet<PlatformAdmin> PlatformAdmins => Set<PlatformAdmin>();
        public virtual DbSet<Plan> Plans => Set<Plan>();
        public virtual DbSet<PriceBook> PriceBooks => Set<PriceBook>();
        public virtual DbSet<PriceBookEntry> PriceBookEntries => Set<PriceBookEntry>();
        public virtual DbSet<PlatformAuditLog> PlatformAuditLogs => Set<PlatformAuditLog>();
        public virtual DbSet<ProvisioningStep> ProvisioningSteps => Set<ProvisioningStep>();

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ModuleConfiguration());
            modelBuilder.ApplyConfiguration(new TenantModuleConfiguration());
            modelBuilder.ApplyConfiguration(new BusinessTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
            modelBuilder.ApplyConfiguration(new EntitlementConfiguration());
            modelBuilder.ApplyConfiguration(new TenantSettingConfiguration());
            modelBuilder.ApplyConfiguration(new PlatformAdminConfiguration());
            modelBuilder.ApplyConfiguration(new PlanConfiguration());
            modelBuilder.ApplyConfiguration(new PriceBookConfiguration());
            modelBuilder.ApplyConfiguration(new PriceBookEntryConfiguration());
            modelBuilder.ApplyConfiguration(new PlatformAuditLogConfiguration());
            modelBuilder.ApplyConfiguration(new ProvisioningStepConfiguration());

            // SP result type — keyless, and not a real table.
            modelBuilder.Entity<SPSupplierLedgerResult>().HasNoKey().ToView(null);

            // Global soft-delete filter for every entity that has a Deleted flag
            // (covers the non-tenant-scoped ones like Tenant). The ITenantScoped
            // loop below then REPLACES it with a combined tenant + soft-delete filter.
            modelBuilder.ApplyGlobalFilter<bool>("Deleted", false);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                    continue;

                var e = Expression.Parameter(entityType.ClrType, "e");

                // e => EF.Property<Guid>(e, "TenantId") == this._currentTenantId
                var tenantId = EfProperty<Guid>(e, "TenantId");
                Expression body = Expression.Equal(
                    tenantId,
                    Expression.Field(Expression.Constant(this), CurrentTenantIdField));

                // ITenantSharable: also match rows shared across every tenant
                // (the standard chart-of-accounts skeleton).
                if (typeof(ITenantSharable).IsAssignableFrom(entityType.ClrType))
                {
                    body = Expression.OrElse(body, Expression.Equal(
                        tenantId,
                        Expression.Constant(TenancyConstants.SystemTenantId)));
                }

                // ... && EF.Property<bool>(e, "Deleted") == false
                // (every ITenantScoped entity carries a Deleted flag; keep the
                //  soft-delete filter that ApplyGlobalFilter set for these types)
                if (entityType.FindProperty("Deleted") is not null)
                {
                    body = Expression.AndAlso(body, Expression.Equal(
                        EfProperty<bool>(e, "Deleted"),
                        Expression.Constant(false)));
                }

                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(Expression.Lambda(body, e));
            }
        }

        private static MethodCallExpression EfProperty<TProperty>(Expression parameter, string propertyName)
            => Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { typeof(TProperty) },
                parameter, Expression.Constant(propertyName));
    }
}
