using Microsoft.Extensions.DependencyInjection;

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
    
    public virtual Task MigrateAsync(IServiceProvider serviceProvider)
    {
        // Default: no migration needed
        return Task.CompletedTask;
    }
}
