using System.Linq.Expressions;
using System.Reflection;
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

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
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
                Expression body = Expression.Equal(
                    EfProperty<Guid>(e, "TenantId"),
                    Expression.Field(Expression.Constant(this), CurrentTenantIdField));

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
