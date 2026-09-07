using System.Linq.Expressions;
using System.Reflection;
using Application.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Data
{
    /// <summary>
    /// Hand-authored partial of the scaffolded <see cref="DataContext"/> that adds
    /// automatic per-tenant isolation. Kept in a separate file so an EF Power Tools
    /// re-scaffold (which regenerates <c>Data/DataContext.cs</c>) cannot wipe it.
    ///
    /// The re-scaffold keeps its single-argument constructor; runtime DI still
    /// selects the tenant-aware overload below because <see cref="ITenantContext"/>
    /// is registered and it is the greedier constructor. See docs/saas-platform-plan.md §11.
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
