using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;
using MultitenantPerDb.Core.Domain;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

/// <summary>
/// Factory interface for creating TenancyDbContext
/// </summary>
public interface ITenancyDbContextFactory : ITenantDbContextFactory<TenancyDbContext>
{
}

/// <summary>
/// Creates TenancyDbContext for master database operations
/// No tenant resolution required - always uses master connection string
/// Implements ICanAccessDbContext as infrastructure component creating DbContext
/// Synchronous factory - DbContext creation is CPU-bound operation
/// </summary>
public class TenancyDbContextFactory : ITenancyDbContextFactory, ITenantDbContextFactory<TenancyDbContext>, ICanAccessDbContext
{
    private readonly IConfiguration _configuration;

    public TenancyDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TenancyDbContext CreateDbContext()
    {
        var connectionString = _configuration.GetConnectionString(TenancyConstants.ConnectionStringKeys.TenantConnection);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Master database connection string 'TenantConnection' not found.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<TenancyDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var context = new TenancyDbContext(optionsBuilder.Options);
        return context;
    }
}
