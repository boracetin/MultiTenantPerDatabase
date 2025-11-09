using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MultitenantPerDb.Core.Infrastructure;

/// <summary>
/// Base abstract class for modules with default implementations
/// </summary>
public abstract class ModuleBase : IModule
{
    public abstract string Name { get; }
    
    public abstract void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    
    public virtual void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Default: no middleware
    }
    
    /// <summary>
    /// Override this to return the DbContext types that should be migrated on startup
    /// Default: no contexts to migrate
    /// </summary>
    protected virtual Type[] GetMigrationContextTypes() => Array.Empty<Type>();
    
    public virtual async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        var contextTypes = GetMigrationContextTypes();
        
        if (contextTypes.Length == 0)
        {
            // No migration needed
            return;
        }
        
        var logger = serviceProvider.GetRequiredService<ILogger<IModule>>();
        
        foreach (var contextType in contextTypes)
        {
            try
            {
                logger.LogInformation("[{ModuleName}] Checking migrations for {ContextType}...", Name, contextType.Name);
                
                var context = serviceProvider.GetRequiredService(contextType) as DbContext;
                if (context == null)
                {
                    logger.LogWarning("[{ModuleName}] Could not resolve {ContextType} as DbContext", Name, contextType.Name);
                    continue;
                }
                
                // Check if database exists and has pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("[{ModuleName}] Applying {Count} pending migrations to {ContextType}...", 
                        Name, pendingMigrations.Count(), contextType.Name);
                    await context.Database.MigrateAsync();
                    logger.LogInformation("[{ModuleName}] ✓ {ContextType} migrated successfully", Name, contextType.Name);
                }
                else
                {
                    logger.LogInformation("[{ModuleName}] {ContextType} is up to date", Name, contextType.Name);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{ModuleName}] Failed to migrate {ContextType}", Name, contextType.Name);
                throw;
            }
        }
    }
}
