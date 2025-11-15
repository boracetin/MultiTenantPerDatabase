using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;
using MultitenantPerDb.Core.Domain;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

/// <summary>
/// Creates TenancyDbContext for master database operations
/// No tenant resolution required - always uses master connection string
/// </summary>
public class ModuleDbContextFactory : IModuleDbContextFactory<TenancyDbContext>, ICanAccessDbContext
{
    private readonly IConfiguration _configuration;

    public ModuleDbContextFactory(IConfiguration configuration)
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
