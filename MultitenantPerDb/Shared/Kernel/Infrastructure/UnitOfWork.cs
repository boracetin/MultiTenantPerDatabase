using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

/// <summary>
/// Unit of Work implementation for managing transactions and repositories
/// Uses generic Repository<T> pattern for all entities
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ITenantDbContextFactory _dbContextFactory;
    private ApplicationDbContext? _context;
    private readonly Dictionary<Type, object> _repositories;
    private bool _disposed;

    public UnitOfWork(ITenantDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _repositories = new Dictionary<Type, object>();
    }

    private async Task<ApplicationDbContext> GetContextAsync()
    {
        if (_context == null)
        {
            _context = await _dbContextFactory.CreateDbContextAsync();
        }
        return _context;
    }

    public IRepository<T> GetGenericRepository<T>() where T : class
    {
        var repositoryType = typeof(IRepository<T>);

        if (_repositories.ContainsKey(repositoryType))
        {
            return (IRepository<T>)_repositories[repositoryType];
        }

        // Context'i senkron olarak almak için Task.Run kullanıyoruz
        var context = GetContextAsync().GetAwaiter().GetResult();

        // Create Repository<T> instance
        var repositoryInstance = new Repository<T>(context);

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
