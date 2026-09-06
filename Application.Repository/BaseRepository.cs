
using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Core.PermissionHelpers;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BaseRepository(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IQueryable<T> TableWithoutTenant()
        {
            return _context.Set<T>().AsNoTracking();
        }
        public IQueryable<T> TableNoTracking()
        {
            Guid? tenantId = _httpContextAccessor?.HttpContext?.GetTenantId();
            if (tenantId.HasValue) return _context.Set<T>().AsNoTracking().Where(ExpressionGenerator.CreateEqualityExpression<T>("TenantId", tenantId.Value));
            else throw new BadRequestException("Tenant Not Found!!!");
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