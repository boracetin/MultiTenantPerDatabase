using Microsoft.Extensions.DependencyInjection;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

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
}
