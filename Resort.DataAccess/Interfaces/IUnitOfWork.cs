using Resort.DataAccess.Interfaces;

namespace Resort.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> CommitAsync();
        Task<int> CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
