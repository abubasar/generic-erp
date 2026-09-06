using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Core.Interfaces
{
    public interface IUnitOfWork
    {
        Task<bool> SaveChangesAsync();
        Task<bool> RefreshTokenSaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        IBaseRepository<T> Repository<T>() where T : class;
    }
}
