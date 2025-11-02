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
        
        // Authentication service (DEPRECATED - use LoginCommandHandler instead)
        services.AddScoped<Application.Services.IAuthService, Application.Services.AuthService>();
        
        // NOTE: UserRepository NOT registered in DI anymore
        // Users are now tenant-specific and created per-request in LoginCommandHandler
        // Each tenant has its own ApplicationDbContext with different connection string
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Authentication middleware is already configured in Program.cs
    }
}
