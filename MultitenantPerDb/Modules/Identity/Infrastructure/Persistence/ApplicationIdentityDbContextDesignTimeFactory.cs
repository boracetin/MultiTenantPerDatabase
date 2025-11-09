using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;

namespace MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations
/// This is only used by dotnet ef tools, not at runtime
/// </summary>
public class ApplicationIdentityDbContextFactory : IDesignTimeDbContextFactory<ApplicationIdentityDbContext>
{
    public ApplicationIdentityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationIdentityDbContext>();
        
        // Use design-time connection string for migrations (Tenant1Db)
        var connectionString = configuration.GetConnectionString(TenancyConstants.ConnectionStringKeys.DesignTimeTenant1Connection);
        
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationIdentityDbContext(optionsBuilder.Options);
    }
}
