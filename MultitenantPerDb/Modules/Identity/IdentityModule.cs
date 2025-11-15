using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;
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
        services.AddScoped<IModuleDbContextFactory<ApplicationIdentityDbContext>, ModuleDbContextFactory<ApplicationIdentityDbContext>>();
        

        // Manually register UserManager and RoleManager without AddEntityFrameworkStores
        // This avoids eager DbContext resolution during startup
        services.AddScoped<UserManager<IdentityUser>>(sp =>
        {
            var context = sp.GetRequiredService<ApplicationIdentityDbContext>();
            var userStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<IdentityUser>(context);
            var options = sp.GetRequiredService<IOptions<IdentityOptions>>();
            var passwordHasher = new PasswordHasher<IdentityUser>();
            var userValidators = sp.GetServices<IUserValidator<IdentityUser>>();
            var passwordValidators = sp.GetServices<IPasswordValidator<IdentityUser>>();
            var keyNormalizer = sp.GetRequiredService<ILookupNormalizer>();
            var errors = sp.GetRequiredService<IdentityErrorDescriber>();
            var services = sp;
            var logger = sp.GetRequiredService<ILogger<UserManager<IdentityUser>>>();
            
            return new UserManager<IdentityUser>(
                userStore,
                options,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                services,
                logger);
        });
        
        services.AddScoped<RoleManager<IdentityRole>>(sp =>
        {
            var context = sp.GetRequiredService<ApplicationIdentityDbContext>();
            var roleStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<IdentityRole>(context);
            var roleValidators = sp.GetServices<IRoleValidator<IdentityRole>>();
            var keyNormalizer = sp.GetRequiredService<ILookupNormalizer>();
            var errors = sp.GetRequiredService<IdentityErrorDescriber>();
            var logger = sp.GetRequiredService<ILogger<RoleManager<IdentityRole>>>();
            
            return new RoleManager<IdentityRole>(
                roleStore,
                roleValidators,
                keyNormalizer,
                errors,
                logger);
        });
        
        // Configure Identity options
        services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            
            // User settings
            options.User.RequireUniqueEmail = true;
        });
        
        // Register required Identity services
        services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        services.AddScoped<IdentityErrorDescriber>();
        
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
