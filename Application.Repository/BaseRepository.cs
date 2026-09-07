
using Application.Core.Data;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data;
using System.Linq.Expressions;

namespace Application.Infrastructure
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly DataContext _context;
        public BaseRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tenant-scoped read set. The DataContext query filter restricts
        /// <see cref="ITenantScoped"/> entities to the current tenant and hides
        /// soft-deleted rows automatically — no manual WHERE needed. With no
        /// tenant in scope, tenant-scoped queries return nothing.
        /// </summary>
        public IQueryable<T> TableNoTracking()
        {
            return _context.Set<T>().AsNoTracking();
        }

        /// <summary>
        /// Bypasses BOTH the tenant filter and the soft-delete filter. Only for
        /// the pre-authentication paths that have no tenant yet — login, refresh
        /// token, tenant lookup/provisioning. Every caller must constrain the
        /// query itself (by username, token, or an explicit TenantId).
        /// </summary>
        public IQueryable<T> TableUnfiltered()
        {
            return _context.Set<T>().AsNoTracking().IgnoreQueryFilters();
        }
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddAsync(entity);
        }
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                _context.Update(entity);
            });
        }
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var persistent = await _context.Set<T>().FindAsync(id);
            if (persistent is null) throw new NotFoundResultException("Entity not found!");
            await Task.Run(() => _context.Remove(persistent));
        }
        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => _context.Remove(entity));
        }
        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            await Task.Run(() =>
            {
                _context.RemoveRange(this.TableNoTracking().Where(predicate));
            });
        }

        public async Task DeleteAsync(IList<T> entities)
        {
            await Task.Run(() =>
        {
            _context.RemoveRange(entities);
        });
        }
        public async Task<T> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity is null) throw new NotFoundResultException("Entity not found!");
            return entity;
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();
            if (includes is not null)
            {
                query = includes(query);
            }
            var entity = await query.FirstOrDefaultAsync(expression);
            if (entity is null) throw new NotFoundResultException("Entity not found!");
            return entity;
        }
        public async Task<bool> IsExists(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().AnyAsync(expression);
        }

        public async Task<IList<T>> ExecuteStoredProcedureAsync(string storedProcedureName, SqlParameter[] parameters)
        {
            var commandText = $"{storedProcedureName} ";
            var paramNames = new List<string>();
            foreach (var param in parameters)
            {
                paramNames.Add($"@{param.ParameterName}");
            }
            commandText += string.Join(", ", paramNames);
            var result = _context.Set<T>().FromSqlRaw(commandText, parameters);
            return await result.ToListAsync();
        }

    }
}