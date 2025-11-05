using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using MultitenantPerDb.Shared.Kernel.Infrastructure;
using MultitenantPerDb.Shared.Kernel.Application.Behaviors;
using System.Reflection;

namespace MultitenantPerDb.Modules.Identity;

/// <summary>
/// Identity Module - Handles authentication and user management
/// </summary>
public class IdentityModule : ModuleBase
{
    public override string Name => "Identity";

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
            // 3. Validation - Validate before processing
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            // 4. Caching - Check cache before executing
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            // 5. Transaction - Multiple contexts supported
            // - MainDbContext: Login, tenant validation operations
            cfg.AddOpenBehavior(typeof(MainDbTransactionBehavior<,>));
            // - ApplicationDbContext: User CRUD operations (tenant-specific)
            cfg.AddOpenBehavior(typeof(ApplicationDbTransactionBehavior<,>));
        });
        
        // FluentValidation - Register all validators in this module
        services.AddValidatorsFromAssembly(assembly);
        
        // Mapster - Register mapping configurations
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        
        // Authentication service (DEPRECATED - use LoginCommandHandler instead)
        services.AddScoped<Application.Services.IAuthService, Application.Services.AuthService>();
        
        // User Service - Business logic for user operations
        // Uses Repository<User> for data access
        services.AddScoped<Application.Services.IUserService, Application.Services.UserService>();
        
        // NOTE: Repository<User> is registered in Shared.Kernel but requires ApplicationDbContext per tenant
        // ApplicationDbContext is tenant-specific and created with tenant's connection string
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Authentication middleware is already configured in Program.cs
    }
}
