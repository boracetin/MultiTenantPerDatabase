using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Identity.Infrastructure.Services;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Core.Application.Behaviors;
using MultitenantPerDb.Core.Domain;
using System.Reflection;
using static MultitenantPerDb.Modules.Identity.Domain.Constants.IdentityConstants;

namespace MultitenantPerDb.Modules.Identity;

/// <summary>
/// Identity Module - Handles authentication with ASP.NET Core Identity
/// </summary>
public class IdentityModule : ModuleBase
{
    public override string Name => ModuleConstants.ModuleName;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // DbContext Factory - Runtime'da tenant bazlı ApplicationIdentityDbContext oluşturur
        services.AddScoped<ITenantDbContextFactory<ApplicationIdentityDbContext>, IdentityDbContextFactory>();
        
        // Register ApplicationIdentityDbContext as scoped service using factory
        // This allows ASP.NET Core Identity to resolve the DbContext
        services.AddScoped<ApplicationIdentityDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<ITenantDbContextFactory<ApplicationIdentityDbContext>>();
            return factory.CreateDbContext();
        });
        
        // ASP.NET Core Identity - Configure Identity services WITHOUT Cookie Authentication
        // Using AddIdentityCore instead of AddIdentity to avoid cookie-based authentication
        // JWT Bearer authentication is configured in Program.cs
        services.AddIdentityCore<IdentityUser>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            
            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
        .AddDefaultTokenProviders();
        
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
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // Authentication middleware is already configured in Program.cs
    }
}
