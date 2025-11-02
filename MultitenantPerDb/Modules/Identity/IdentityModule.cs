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
        
        // Authentication service
        services.AddScoped<Application.Services.IAuthService, Application.Services.AuthService>();
        
        // User Repository - Uses TenantDbContext for Master DB access (tenant-independent)
        services.AddScoped<Domain.Repositories.IUserRepository, Infrastructure.Persistence.UserRepository>();
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Authentication middleware is already configured in Program.cs
    }
}
