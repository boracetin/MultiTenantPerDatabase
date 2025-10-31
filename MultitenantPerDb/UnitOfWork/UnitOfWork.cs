using Microsoft.EntityFrameworkCore.Storage;
using MultitenantPerDb.Data;
using MultitenantPerDb.Services;

namespace MultitenantPerDb.UnitOfWork;

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

    public TRepository GetRepository<TRepository>() where TRepository : class
    {
        var type = typeof(TRepository);

        if (_repositories.ContainsKey(type))
        {
            return (TRepository)_repositories[type];
        }

        // Context'i senkron olarak almak için Task.Run kullanıyoruz
        // Not: Bu ideal değil ama constructor'da async kullanamıyoruz
        var context = GetContextAsync().GetAwaiter().GetResult();

        // Repository instance'ını oluştur
        var repositoryInstance = Activator.CreateInstance(type, context) 
            ?? throw new InvalidOperationException($"Repository {type.Name} oluşturulamadı");

        _repositories.Add(type, repositoryInstance);
        return (TRepository)repositoryInstance;
    }

    public async Task<int> SaveChangesAsync()
    {
        var context = await GetContextAsync();

        // Transaction kullanarak kaydet
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var result = await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
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

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_context != null)
            {
                await _context.DisposeAsync();
            }
            _repositories.Clear();
        }
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
