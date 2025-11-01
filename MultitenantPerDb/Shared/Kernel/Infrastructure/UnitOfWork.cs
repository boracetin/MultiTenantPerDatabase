using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

/// <summary>
/// Unit of Work implementation for managing transactions and repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ITenantDbContextFactory _dbContextFactory;
    private ApplicationDbContext? _context;
    private readonly Dictionary<Type, object> _repositories;
    private bool _disposed;
    private static readonly Dictionary<Type, Type> _repositoryMappings = new();

    static UnitOfWork()
    {
        // Scan all loaded assemblies for repository implementations
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && !type.IsAbstract)
                    {
                        var interfaces = type.GetInterfaces();
                        foreach (var @interface in interfaces)
                        {
                            if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IRepository<>))
                            {
                                continue; // Skip base IRepository<T>
                            }
                            
                            if (@interface.Name.StartsWith("I") && @interface.Name.EndsWith("Repository"))
                            {
                                _repositoryMappings[@interface] = type;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Skip assemblies that can't be scanned
            }
        }
    }

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
        var interfaceType = typeof(TRepository);

        if (_repositories.ContainsKey(interfaceType))
        {
            return (TRepository)_repositories[interfaceType];
        }

        // Context'i senkron olarak almak için Task.Run kullanıyoruz
        var context = GetContextAsync().GetAwaiter().GetResult();

        // Find implementation type for the interface
        if (!_repositoryMappings.TryGetValue(interfaceType, out var implementationType))
        {
            throw new InvalidOperationException(
                $"Repository implementation for {interfaceType.Name} not found. " +
                $"Ensure the repository class implements {interfaceType.Name}.");
        }

        // Repository instance'ını oluştur
        var repositoryInstance = Activator.CreateInstance(implementationType, context) 
            ?? throw new InvalidOperationException($"Repository {implementationType.Name} oluşturulamadı");

        _repositories.Add(interfaceType, repositoryInstance);
        return (TRepository)repositoryInstance;
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
