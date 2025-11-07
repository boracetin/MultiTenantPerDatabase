using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Services;

/// <summary>
/// Factory interface for creating MainDbContext
/// </summary>
public interface IMainDbContextFactory : ITenantDbContextFactory<MainDbContext>
{
}

/// <summary>
/// Creates MainDbContext for master database operations
/// No tenant resolution required - always uses master connection string
/// Implements ICanAccessDbContext as infrastructure component creating DbContext
/// </summary>
public class MainDbContextFactory : IMainDbContextFactory, ITenantDbContextFactory<MainDbContext>, ICanAccessDbContext
{
    private readonly IConfiguration _configuration;

    public MainDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<MainDbContext> CreateDbContextAsync()
    {
        var connectionString = _configuration.GetConnectionString("TenantConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Master database connection string 'TenantConnection' not found.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<MainDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var context = new MainDbContext(optionsBuilder.Options);
        return Task.FromResult(context);
    }
}
