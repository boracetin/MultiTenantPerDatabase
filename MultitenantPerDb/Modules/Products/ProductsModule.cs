using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Products;

/// <summary>
/// Products Module - Handles product management
/// </summary>
public class ProductsModule : ModuleBase
{
    public override string Name => "Products";

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Note: Repositories are created dynamically by UnitOfWork using Activator.CreateInstance
        // No need to register them in DI container
        
        // Product background jobs
        services.AddScoped<Application.Jobs.ProductBackgroundJob>();
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No specific middleware for Products module
    }
}

