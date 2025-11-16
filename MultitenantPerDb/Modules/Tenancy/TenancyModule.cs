using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Infrastructure;
using Core.Infrastructure.Persistence.Concrete;
using MultitenantPerDb.Core.Application.Interfaces;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Hubs;
using MultitenantPerDb.Core.Infrastructure.Tenancy.Contract;
using MultitenantPerDb.Core.Infrastructure.Services;
using MultitenantPerDb.Modules.Tenancy.Application.Services;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;
using MultitenantPerDb.Core.Application.Module.Concrete;

namespace MultitenantPerDb.Modules.Tenancy;

/// <summary>
/// Tenancy Module - Handles multi-tenant infrastructure
/// </summary>
public class TenancyModule : ModuleBase
{

    public override string Name => "Tenancy";

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Tenant resolution services - MOVED TO CORE (registered in Program.cs)
        // services.AddScoped<ITenantResolver, TenantResolver>();
        
        // Tenant service - Uses UnitOfWork<TenancyDbContext>
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IModuleDbContextFactory<TenancyDbContext>, MainDbContextFactory<TenancyDbContext>>();
        
        // Register MainDbContext<Tenant> factory for ApplicationDbContextFactory dependency
        services.AddScoped<IModuleDbContextFactory<MainDbContext<Tenant>>, MainDbContextFactory<MainDbContext<Tenant>>>();
    
        // SignalR Hub Notification Service
        services.AddScoped<TenantHubNotificationService>();
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Tenant middleware will be configured here
        app.UseMiddleware<Infrastructure.Middleware.TenantMiddleware>();
    }

    /// <summary>
    /// Specify which contexts should be migrated on startup
    /// </summary>
    protected override Type[] GetMigrationContextTypes()
    {
        return new[] { typeof(TenancyDbContext) };
    }
}
