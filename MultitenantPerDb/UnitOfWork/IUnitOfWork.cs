namespace MultitenantPerDb.UnitOfWork;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    TRepository GetRepository<TRepository>() where TRepository : class;
    Task<int> SaveChangesAsync();
}
