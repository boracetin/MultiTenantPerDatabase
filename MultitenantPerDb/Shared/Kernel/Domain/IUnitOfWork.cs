namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Unit of Work interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets a generic Repository<T> for entity T
    /// All entities use generic Repository<T> pattern
    /// </summary>
    IRepository<T> GetGenericRepository<T>() where T : class;
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
