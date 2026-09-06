using System.Linq.Expressions;

namespace Application.Core.Common
{
    public abstract class BaseRequestModel<T> where T : class
    {
        protected Expression<Func<T, bool>>
            ExpressionObject = e => true; // protected:-ai property ta baserequestmodel er bassa jara tara pabe

        public int Page { get; set; } = 0;

        public int RowsPerPage { get; set; } = 5;

        public string OrderBy { get; set; } = "CreatedOn";

        public bool IsAscending { get; set; }

        public string? Keyword { get; set; }
        public abstract Expression<Func<T, bool>> GetExpression();

        public IQueryable<T> SkipAndTake(IQueryable<T> queryable)
        {
            if (Page != -1) queryable = queryable.Skip(Page * RowsPerPage).Take(RowsPerPage);
            return queryable;
        }

        public virtual IQueryable<T> IncludeParents(IQueryable<T> queryable)
        {
            return queryable;
        }


        //creating dynamic lambda expression
        public IQueryable<T> CreateOrderByQueryable(IQueryable<T> queryable)
        {
            // Build the lambda expression for the order-by clause
            ParameterExpression parameter = Expression.Parameter(queryable.ElementType, "");
            MemberExpression property = Expression.Property(parameter, OrderBy);
            LambdaExpression lambda = Expression.Lambda(property, parameter);
            // Use the lambda expression to sort the collection
            Expression methodCallExpression = Expression.Call(typeof(Queryable),
                                  IsAscending ? "OrderBy" : "OrderByDescending",
                                  new Type[] { queryable.ElementType, property.Type },
                                  queryable.Expression, Expression.Quote(lambda));

            return queryable.Provider.CreateQuery<T>(methodCallExpression);
        }
        public Func<IQueryable<T>, IOrderedQueryable<T>> OrderByFunc() //order by lambda expression generate kore dibe
        {
            string propertyName = OrderBy;
            bool ascending = IsAscending;
            var source = Expression.Parameter(typeof(IQueryable<T>), "source");
            var item = Expression.Parameter(typeof(T), "item");
            var member = Expression.Property(item, propertyName);
            var selector = Expression.Quote(Expression.Lambda(member, item));
            var body = Expression.Call(
                typeof(Queryable), ascending ? "OrderBy" : "OrderByDescending",
                new[] { item.Type, member.Type },
                source, selector);
            var expr = Expression.Lambda<Func<IQueryable<T>, IOrderedQueryable<T>>>(body, source);
            var func = expr.Compile();
            return func;
        }

    }
}
//Dynamic Linq Query
//This ExpressionHelper class provides extension methods And and Or for combining expressions with the logical AND and OR operators, 
//respectively. It uses the Compose method to merge two expressions with a merge function, replacing the parameters
//of the second expression with the corresponding parameters of the first.The ParameterRebinder class helps to replace parameters.
public static class ExpressionHelper
{
    public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
        Func<Expression, Expression, Expression> merge)
    {
        // build parameter map (from parameters of second to parameters of first)
        var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] })
            .ToDictionary(p => p.s, p => p.f);

        // replace parameters in the second lambda expression with parameters from the first
        var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

        // apply composition of lambda expression bodies to parameters from the first expression 
        return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.And);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.Or);
    }



}

public class ParameterRebinder : ExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> map;

    public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
    {
        this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
    }

    public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map,
        Expression exp)
    {
        return new ParameterRebinder(map).Visit(exp);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
        ParameterExpression? replacement;
        if (map.TryGetValue(p, out replacement))
        {
            p = replacement;
        }
        return base.VisitParameter(p);
    }
}

