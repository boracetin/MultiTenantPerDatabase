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
        services.AddScoped<Infrastructure.Services.IApplicationDbContextFactory, Infrastructure.Services.ApplicationDbContextFactory>();
        
        // Tenant service - Uses MainDbContext directly (not via UnitOfWork)
        services.AddScoped<Application.Services.ITenantService, Application.Services.TenantService>();
        
        // Main database context (master database)
        services.AddDbContext<Infrastructure.Persistence.MainDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("TenantConnection")));
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Tenant middleware will be configured here
        app.UseMiddleware<Infrastructure.Middleware.TenantMiddleware>();
    }
}
