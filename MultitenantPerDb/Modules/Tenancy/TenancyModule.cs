using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Tenancy;

/// <summary>
/// Tenancy Module - Handles multi-tenant infrastructure
/// </summary>
public class TenancyModule : ModuleBase
{
    public override string Name => "Tenancy";

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Tenant resolution services
        services.AddScoped<Infrastructure.Services.ITenantResolver, Infrastructure.Services.TenantResolver>();
        services.AddScoped<Infrastructure.Services.ITenantDbContextFactory, Infrastructure.Services.TenantDbContextFactory>();
        
        // Note: TenantRepository is created dynamically by UnitOfWork
        
        // Tenant database context (master database)
        services.AddDbContext<Infrastructure.Persistence.TenantDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("TenantConnection")));
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Tenant middleware will be configured here
        app.UseMiddleware<Infrastructure.Middleware.TenantMiddleware>();
    }
}
