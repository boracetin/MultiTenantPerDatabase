namespace MultitenantPerDb.Domain.Repositories;

/// <summary>
/// Unit of Work interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    TRepository GetRepository<TRepository>() where TRepository : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
