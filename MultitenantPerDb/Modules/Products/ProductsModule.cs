using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using MultitenantPerDb.Shared.Kernel.Infrastructure;
using MultitenantPerDb.Shared.Kernel.Application.Behaviors;
using MultitenantPerDb.Modules.Products.Application.Services;
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
            // 7. Transaction - Wrap commands in transaction (with ApplicationDbContext, innermost)
            cfg.AddOpenBehavior(typeof(ApplicationDbTransactionBehavior<,>));
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
        // Uses Repository<Product> for data access
        services.AddScoped<IProductService, Application.Services.ProductService>();
        
        // NOTE: Repository<Product> is registered in Shared.Kernel but requires ApplicationDbContext per tenant
        // ApplicationDbContext is tenant-specific and created with tenant's connection string via TenantDbContextFactory
        
        // Product background jobs
        services.AddScoped<Application.Jobs.ProductBackgroundJob>();
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No specific middleware for Products module
    }
}


