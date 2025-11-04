namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Unit of Work interface for managing transactions
/// Works with any DbContext type (generic TDbContext support)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets a generic Repository<TEntity> for entity TEntity
    /// TEntity: Entity type (Product, User, Tenant, etc.)
    /// Works with any DbContext injected via factory
    /// </summary>
    IRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class;
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
