using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using MultitenantPerDb.Shared.Kernel.Infrastructure;
using MultitenantPerDb.Shared.Kernel.Application.Behaviors;
using System.Reflection;

namespace MultitenantPerDb.Modules.Products;

/// <summary>
/// Products Module - Handles product management
/// </summary>
public class ProductsModule : ModuleBase
{
    public override string Name => "Products";

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // MediatR - Register all handlers in this module
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            
            // Register pipeline behaviors (order matters!)
            // 1. Logging - Log everything first
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            // 2. Validation - Validate before processing
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            // 3. Caching - Check cache before executing
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            // 4. Transaction - Wrap commands in transaction
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });
        
        // FluentValidation - Register all validators in this module
        services.AddValidatorsFromAssembly(assembly);
        
        // Mapster - Register mapping configurations
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        
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

