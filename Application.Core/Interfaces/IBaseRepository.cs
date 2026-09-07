using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Core.Interfaces

{
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>Bypasses the tenant and soft-delete filters. Pre-auth paths only — see <c>BaseRepository</c>.</summary>
        IQueryable<T> TableUnfiltered();
        IQueryable<T> TableNoTracking();
        Task<bool> IsExists(Expression<Func<T, bool>> expression);
        Task<T> FindAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T> FindAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Expression<Func<T, bool>> predicate);
        Task DeleteAsync(IList<T> entities);
        Task<IList<T>> ExecuteStoredProcedureAsync(string storedProcedureName, SqlParameter[] parameters);

    }
}