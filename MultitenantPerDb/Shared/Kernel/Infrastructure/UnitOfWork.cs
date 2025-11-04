using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

/// <summary>
/// Unit of Work implementation with generic TDbContext and TEntity support
/// Provides transaction management and repository creation for any DbContext
/// Uses generic Repository<TEntity, TDbContext> pattern
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ITenantDbContextFactory _dbContextFactory;
    private DbContext? _context;
    private readonly Dictionary<Type, object> _repositories;
    private bool _disposed;

    public UnitOfWork(ITenantDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _repositories = new Dictionary<Type, object>();
    }

    private async Task<DbContext> GetContextAsync()
    {
        if (_context == null)
        {
            _context = await _dbContextFactory.CreateDbContextAsync();
        }
        return _context;
    }

    public IRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class
    {
        var repositoryType = typeof(IRepository<TEntity>);

        if (_repositories.ContainsKey(repositoryType))
        {
            return (IRepository<TEntity>)_repositories[repositoryType];
        }

        // Get DbContext (can be any DbContext: ApplicationDbContext, MainDbContext, etc.)
        var context = GetContextAsync().GetAwaiter().GetResult();

        // Create Repository<TEntity> instance with generic DbContext
        var repositoryInstance = new Repository<TEntity>(context);

        _repositories.Add(repositoryType, repositoryInstance);
        return repositoryInstance;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync();

        // Transaction kullanarak kaydet
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
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
