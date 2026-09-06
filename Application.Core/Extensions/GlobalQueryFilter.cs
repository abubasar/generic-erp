using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Core.Extensions
{
    public static class GlobalQueryFilter
    {
        public static void  ApplyGlobalFilter<T> (this ModelBuilder modelBuilder,
            string propertyName,T value)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var foundProperty = entityType.FindProperty(propertyName);
                if (foundProperty is not null && foundProperty.ClrType==typeof(T))
                {
                    var newParam = Expression.Parameter(entityType.ClrType);
                    var filter = Expression.Lambda(Expression.Equal(Expression.Property(newParam, propertyName),
                        Expression.Constant(value)), newParam);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }
    }
}
