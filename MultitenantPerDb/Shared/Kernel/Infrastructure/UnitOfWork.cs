using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

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

    private async Task<TDbContext> GetOrCreateContextAsync()
    {
        if (_context == null)
        {
            _context = await _dbContextFactory.CreateDbContextAsync();
        }
        return _context;
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IEntity
    {
        var repositoryType = typeof(IRepository<TEntity>);

        if (_repositories.ContainsKey(repositoryType))
        {
            return (IRepository<TEntity>)_repositories[repositoryType];
        }

        // Get DbContext (can be any DbContext: ApplicationDbContext, MainDbContext, etc.)
        var context = GetOrCreateContextAsync().GetAwaiter().GetResult();

        // Create Repository<TEntity> instance with generic DbContext
        var repositoryInstance = new Repository<TEntity>(context);

        _repositories.Add(repositoryType, repositoryInstance);
        return repositoryInstance;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync();
        // Transaction yönetimi TransactionBehavior'da yapılıyor
        // Burada sadece değişiklikleri kaydet
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync();
        await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync();
        if (context.Database.CurrentTransaction != null)
        {
            await context.SaveChangesAsync(cancellationToken);
            await context.Database.CurrentTransaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync();
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
