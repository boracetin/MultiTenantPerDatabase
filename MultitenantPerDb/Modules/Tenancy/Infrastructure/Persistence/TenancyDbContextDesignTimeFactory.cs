using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations
/// This is only used by dotnet ef tools, not at runtime
/// </summary>
public class TenancyDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TenancyDbContext>
{
    public TenancyDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TenancyDbContext>();
        
        // Use connection string from appsettings.json (TenantMasterDb)
        var connectionString = configuration.GetConnectionString(TenancyConstants.ConnectionStringKeys.TenantConnection);
        
        optionsBuilder.UseSqlServer(connectionString);

        return new TenancyDbContext(optionsBuilder.Options);
    }
}
