using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Infrastructure.Services;

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
        services.AddScoped<Application.Services.ITenantService, Application.Services.TenantService>();
        
        // Factory for creating TenancyDbContext (master database)
        services.AddScoped<ITenantDbContextFactory<TenancyDbContext>, Infrastructure.Services.TenancyDbContextFactory>();
        
        // TenancyDbContext registration via factory pattern
        services.AddScoped<TenancyDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<ITenantDbContextFactory<TenancyDbContext>>();
            return factory.CreateDbContext();
        });
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
