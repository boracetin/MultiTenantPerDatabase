using Microsoft.EntityFrameworkCore;

namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Unit of Work interface for managing transactions and repositories
/// Generic interface - works with specific DbContext type (TDbContext)
/// Transaction yönetimi TransactionBehavior tarafından yapılır
/// </summary>
public interface IUnitOfWork<TDbContext> : IDisposable
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets a Repository<TEntity> for entity TEntity
    /// TEntity: Entity type (Product, User, Tenant, etc.)
    /// Works with any DbContext injected via factory
    /// </summary>
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IEntity;
    
    /// <summary>
    /// Saves all changes to the database
    /// Transaction management is handled by TransactionBehavior
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a new database transaction
    /// Used by TransactionBehavior for explicit transaction control
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Commits the current transaction
    /// Saves changes and commits in one atomic operation
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the current transaction
    /// Discards all changes made within the transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
