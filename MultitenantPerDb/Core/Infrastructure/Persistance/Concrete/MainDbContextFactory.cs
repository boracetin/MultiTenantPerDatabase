using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Infrastructure.Tenancy.Contract;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Domain.Entity.Concrete;
using MultitenantPerDb.Core.Domain.Constants;

namespace Core.Infrastructure.Persistence.Concrete;

/// <summary>
/// Generic factory for creating tenant-specific DbContext instances with caching support
/// Caches tenant information to avoid database lookups on every request
/// Uses Activator.CreateInstance to instantiate DbContext - no need for module-specific factories
/// </summary>
public class MainDbContextFactory<TContext> : IModuleDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MainDbContextFactory<TContext>> _logger;

    public MainDbContextFactory(
        IConfiguration configuration,
        ILogger<MainDbContextFactory<TContext>> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public TContext CreateDbContext()
    {
        var connectionString = _configuration.GetConnectionString(TenancyConstants.MainConnectionString);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Master database connection string 'TenantConnection' not found.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var context = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;
        return context;
    }
}
