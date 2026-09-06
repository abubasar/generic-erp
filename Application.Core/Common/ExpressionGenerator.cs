using System.Linq.Expressions;

namespace Application.Core.Common
{
    public  class ExpressionGenerator
    {
        public static Expression<Func<TEntity, bool>> CreateEqualityExpression<TEntity>(string propertyName, Guid value)
        {
            var param = Expression.Parameter(typeof(TEntity), "p");
            var member = Expression.Property(param, propertyName);
            var constant = Expression.Constant(value);
            var body = Expression.Equal(member, constant);
            return Expression.Lambda<Func<TEntity, bool>>(body, param);
        }

        
    }
}
