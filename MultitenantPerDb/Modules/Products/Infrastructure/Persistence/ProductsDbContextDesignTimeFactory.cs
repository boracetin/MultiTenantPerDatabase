using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;

namespace MultitenantPerDb.Modules.Products.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations
/// This is ONLY used by 'dotnet ef migrations add' command to generate migration files.
/// At runtime, ProductsDbContext is created with tenant-specific connection string from TenancyDbContext.
/// The connection string here points to a template/sample tenant database (Tenant1Db) for:
/// 1. Migration file generation (dotnet ef migrations add)
/// 2. Optional manual migration testing (dotnet ef database update)
/// In production, migrations are applied automatically to each tenant database at runtime.
/// </summary>
public class ProductsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ProductsDbContext>
{
    public ProductsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ProductsDbContext>();
        
        // Use design-time connection string for migrations (Tenant1Db)
        var connectionString = configuration.GetConnectionString(TenancyConstants.ConnectionStringKeys.DesignTimeTenant1Connection);
        
        optionsBuilder.UseSqlServer(connectionString);

        return new ProductsDbContext(optionsBuilder.Options);
    }
}
