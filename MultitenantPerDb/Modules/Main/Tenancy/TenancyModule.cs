using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Main.Tenancy;

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
        
        // Generic factory registrations for UnitOfWork
        services.AddScoped<ITenantDbContextFactory<ApplicationDbContext>, Infrastructure.Services.ApplicationDbContextFactory>();
        services.AddScoped<ITenantDbContextFactory<MainDbContext>, Infrastructure.Services.MainDbContextFactory>();
        
        // Tenant service - Uses MainDbContext directly (not via UnitOfWork)
        services.AddScoped<Application.Services.ITenantService, Application.Services.TenantService>();
        
        // Main database context (master database)
        services.AddDbContext<MainDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("TenantConnection")));
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Tenant middleware will be configured here
        app.UseMiddleware<Infrastructure.Middleware.TenantMiddleware>();
    }
}
