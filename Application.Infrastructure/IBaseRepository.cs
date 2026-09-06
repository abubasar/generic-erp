using System.Linq.Expressions;

namespace Application.Infrastructure

{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> Table();
        IQueryable<T> TableNoTracking();
        Task<bool> IsExists(Expression<Func<T, bool>> expression = null);
        Task<T> FindAsync(string id, CancellationToken cancellationToken = default);
        Task<T> FindAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task DeleteAsync(Expression<Func<T, bool>> predicate);
        Task DeleteAsync(IList<T> entities);

    }
}