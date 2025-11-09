using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;

namespace MultitenantPerDb.Modules.User.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations
/// This is only used by dotnet ef tools, not at runtime
/// </summary>
public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        
        // Use design-time connection string for migrations (Tenant1Db)
        var connectionString = configuration.GetConnectionString(TenancyConstants.ConnectionStringKeys.DesignTimeTenant1Connection);
        
        optionsBuilder.UseSqlServer(connectionString);

        return new UserDbContext(optionsBuilder.Options);
    }
}
