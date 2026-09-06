
using Application.Core.Data;
using System.Linq.Expressions;

namespace Application.Infrastructure
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly VET_TESTContext _context;
        public BaseRepository(VET_TESTContext context)
        {
            _context = context;
        }

        public IQueryable<T> Table()
        {
            return _context.Set<T>();
        }
        public IQueryable<T> TableNoTracking()
        {
            return _context.Set<T>().AsNoTracking();
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


        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var persistent = await _context.Set<T>().FindAsync(id);
            _context.Remove(persistent);
        }
        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            await Task.Run(() =>
            {
                _context.RemoveRange(this.Table().Where(predicate));
            });
        }

        public async Task DeleteAsync(IList<T> entities)
        {
            await Task.Run(() =>
        {
            _context.RemoveRange(entities);
        });
        }
        public async Task<T> FindAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes != null)
            {
                query = includes(query);
            }
            return await query.FirstOrDefaultAsync(expression);
        }
        public async Task<bool> IsExists(Expression<Func<T, bool>> expression = null)
        {
            return await _context.Set<T>().AnyAsync(expression);
        }


    }
}