using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Modules.Tenancy.Domain.Constants;

namespace MultitenantPerDb.Modules.Tenancy;

/// <summary>
/// Tenancy Module - Handles multi-tenant infrastructure
/// </summary>
public class TenancyModule : ModuleBase
{
    private readonly ILogger<TenancyModule> _logger;

    public TenancyModule(ILogger<TenancyModule> logger)
    {
        _logger = logger;
    }
    public override string Name => "Tenancy";

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Tenant resolution services
        services.AddScoped<Infrastructure.Services.ITenantResolver, Infrastructure.Services.TenantResolver>();
        
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

    public override async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        try
        {
            _logger.LogInformation("[{ModuleName}] Starting database migration...", Name);
            var context = serviceProvider.GetRequiredService<TenancyDbContext>();
            await context.Database.MigrateAsync();
            _logger.LogInformation("[{ModuleName}] ✓ Database migrated successfully", Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{ModuleName}] Failed to migrate database", Name);
            throw;
        }
    }
}
