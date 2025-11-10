using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Modules.User.Infrastructure.Persistence;

namespace MultitenantPerDb.Modules.User;

/// <summary>
/// User Module - Handles user management in tenant-specific databases
/// </summary>
public class UserModule : ModuleBase
{
    public override string Name => "User";

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register UserDbContext factory for runtime tenant-specific context creation
        services.AddScoped<ITenantDbContextFactory<UserDbContext>, Infrastructure.Services.UserDbContextFactory>();
        
        // Register UnitOfWork for UserDbContext
        services.AddScoped<IUnitOfWork<UserDbContext>, UnitOfWork<UserDbContext>>();
        
        // Register generic repository for User entities
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
    }

    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No middleware needed for User module
    }
}
