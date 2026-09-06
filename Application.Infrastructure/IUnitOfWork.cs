namespace Application.Infrastructure
{ 
    public interface IUnitOfWork
    {
        Task<bool> SaveChangesAsync();
        IBaseRepository<T> Repository<T>() where T : class;
    }
}
