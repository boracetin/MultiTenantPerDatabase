using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Infrastructure.Repository.Contract;
using MultitenantPerDb.Core.Infrastructure.Repository.Concrete;
using MultitenantPerDb.Core.Infrastructure.UnitOfWork.Contract;

namespace MultitenantPerDb.Core.Infrastructure.UnitOfWork.Concrete;

/// <summary>
/// Unit of Work implementation with generic TDbContext support
/// Provides transaction management and repository creation for any DbContext
/// Uses factory pattern for lazy DbContext initialization
/// </summary>
public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _context;
    private readonly Dictionary<Type, object> _repositories;
    private bool _disposed;

    public UnitOfWork(IModuleDbContextFactory<TDbContext> dbContextFactory)
    {
        _context = dbContextFactory.CreateDbContext();
        _repositories = new Dictionary<Type, object>();
    }

    /// <summary>
    /// Get the underlying DbContext - eliminates reflection in TransactionBehavior
    /// </summary>
    public DbContext GetDbContext() => _context;

    public IRepository<TEntity, TId> GetRepository<TEntity, TId>() 
        where TEntity : class, IEntity<TId> 
        where TId : IEquatable<TId>
    {
        var repositoryType = typeof(IRepository<TEntity, TId>);

        if (_repositories.ContainsKey(repositoryType))
        {
            return (IRepository<TEntity, TId>)_repositories[repositoryType];
        }

        // Create Repository<TEntity, TId> instance with generic DbContext
        var repositoryInstance = new Repository<TEntity, TId>(_context);

        _repositories.Add(repositoryType, repositoryInstance);
        return repositoryInstance;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Transaction yönetimi TransactionBehavior'da yapılıyor
        // Burada sadece değişiklikleri kaydet
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _context.Database.CurrentTransaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.Database.CurrentTransaction.RollbackAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
            _repositories.Clear();
        }
        _disposed = true;
    }
}
