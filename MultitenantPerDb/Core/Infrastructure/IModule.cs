namespace MultitenantPerDb.Core.Infrastructure;

/// <summary>
/// Base interface for all modules
/// </summary>
public interface IModule
{
    string Name { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void ConfigureMiddleware(IApplicationBuilder app);
    
    /// <summary>
    /// Migrate module's main databases (not tenant-specific databases)
    /// Called on application startup
    /// </summary>
    Task MigrateAsync(IServiceProvider serviceProvider);
}
