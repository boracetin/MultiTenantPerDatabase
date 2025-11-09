using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;
using MultitenantPerDb.Core.Domain;

namespace MultitenantPerDb.Core.Infrastructure;

/// <summary>
/// Unit of Work implementation with generic TDbContext support
/// Provides transaction management and repository creation for any DbContext
/// Uses factory pattern for lazy DbContext initialization
/// Implements ICanAccessDbContext to explicitly allow DbContext access
/// </summary>
public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext>, ICanAccessDbContext
    where TDbContext : DbContext
{
    private readonly ITenantDbContextFactory<TDbContext> _dbContextFactory;
    private TDbContext? _context;
    private readonly Dictionary<Type, object> _repositories;
    private bool _disposed;

    public UnitOfWork(ITenantDbContextFactory<TDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _repositories = new Dictionary<Type, object>();
    }

    private TDbContext GetOrCreateContext()
    {
        if (_context == null)
        {
            _context = _dbContextFactory.CreateDbContext();
        }
        return _context;
    }

    public IRepository<TEntity, TId> GetRepository<TEntity, TId>() 
        where TEntity : class, IEntity<TId> 
        where TId : IEquatable<TId>
    {
        var repositoryType = typeof(IRepository<TEntity, TId>);

        if (_repositories.ContainsKey(repositoryType))
        {
            return (IRepository<TEntity, TId>)_repositories[repositoryType];
        }

        // Get DbContext (can be any DbContext: ApplicationDbContext, TenancyDbContext, etc.)
        var context = GetOrCreateContext();

        // Create Repository<TEntity, TId> instance with generic DbContext
        var repositoryInstance = new Repository<TEntity, TId>(context);

        _repositories.Add(repositoryType, repositoryInstance);
        return repositoryInstance;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var context = GetOrCreateContext();
        // Transaction yönetimi TransactionBehavior'da yapılıyor
        // Burada sadece değişiklikleri kaydet
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = GetOrCreateContext();
        await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = GetOrCreateContext();
        if (context.Database.CurrentTransaction != null)
        {
            await context.SaveChangesAsync(cancellationToken);
            await context.Database.CurrentTransaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = GetOrCreateContext();
        if (context.Database.CurrentTransaction != null)
        {
            await context.Database.CurrentTransaction.RollbackAsync(cancellationToken);
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
            _context?.Dispose();
            _repositories.Clear();
        }
        _disposed = true;
    }
}
