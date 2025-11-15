using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Core.Application.Behaviors;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Modules.Products.Application.Services;
using MultitenantPerDb.Modules.Products.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Products.Infrastructure.Hubs;
using MultitenantPerDb.Core.Application.Interfaces;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
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
        
        // DbContext Factory - Runtime'da tenant bazlı ProductsDbContext oluşturur
        services.AddScoped<ITenantDbContextFactory<ProductsDbContext>, TenantDbContextFactory<ProductsDbContext>>();
        
        // MediatR - Register all handlers in this module
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            
            // Register pipeline behaviors (order matters!)
            // 1. Exception Handling - Catch all exceptions first (outermost)
            cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            // 2. Logging - Log everything
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            // 3. Authorization - Check permissions and tenant isolation (SECURITY!)
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            // 4. Rate Limiting - Prevent API abuse (SECURITY!)
            cfg.AddOpenBehavior(typeof(RateLimitingBehavior<,>));
            // 5. Validation - Validate before processing
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            // 6. Caching - Check cache before executing
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            // 7. Distributed Transaction - Manage transactions across multiple databases (innermost)
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });
        
        // FluentValidation - Register all validators in this module
        services.AddValidatorsFromAssembly(assembly);
        
        // Mapster - Register mapping configurations
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        
        // Application Services - Domain-specific orchestration
        services.AddScoped<IProductNotificationService, ProductNotificationService>();
        
        // Product Service - Business logic for product operations
        // Uses Repository<Product> with ProductsDbContext for data access
        services.AddScoped<IProductService, Application.Services.ProductService>();
        
        // Product background jobs
        services.AddScoped<Application.Jobs.ProductBackgroundJob>();
        
        // SignalR Hub Notification Service
        services.AddScoped<ProductHubNotificationService>();
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No specific middleware for Products module
    }
}


