namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

/// <summary>
/// Base interface for all modules
/// </summary>
public interface IModule
{
    string Name { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void ConfigureMiddleware(IApplicationBuilder app);
}
