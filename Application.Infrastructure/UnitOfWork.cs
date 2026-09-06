using Application.Core.Common;
using Application.Core.Data;
using Application.Core.PermissionHelpers;

namespace Application.Infrastructure

{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VET_TESTContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        public UnitOfWork(VET_TESTContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }
        public async Task<bool> SaveChangesAsync()
        {
            bool returnValue = true;
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var entries = _context.ChangeTracker.Entries().Where(e =>
                    e.State == EntityState.Added
                   || e.State == EntityState.Modified);

                    foreach (var entityEntry in entries)
                    {
                        entityEntry.Property("UpdatedOn").CurrentValue = DateTimeHelper.Now;
                        entityEntry.Property("UpdatedBy").CurrentValue = _contextAccessor.HttpContext?.User?.GetUserId();

                        if (entityEntry.State == EntityState.Added)
                        {
                            entityEntry.Property("CreatedOn").CurrentValue = DateTimeHelper.Now;
                            entityEntry.Property("CreatedBy").CurrentValue = _contextAccessor.HttpContext?.User?.GetUserId();
                        }
                    }
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                }

                catch (Exception)
                {
                    //Log Exception Handling message                      
                    returnValue = false;
                    await dbContextTransaction.RollbackAsync();
                    throw;

                }
            }

            return returnValue;
        }

        public IBaseRepository<T> Repository<T>() where T : class
        {
            return new BaseRepository<T>(_context);
        }
    }
}
