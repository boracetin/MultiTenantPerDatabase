using System.Reflection;

namespace MultitenantPerDb.Core.Infrastructure;

/// <summary>
/// Extension methods for registering modules
/// </summary>
public static class ModuleExtensions
{
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        var modules = DiscoverModules();
        
        foreach (var module in modules)
        {
            Console.WriteLine($"Registering module: {module.Name}");
            module.ConfigureServices(services, configuration);
        }
        
        // Store modules for later use in middleware configuration
        services.AddSingleton<IEnumerable<IModule>>(modules);
        
        return services;
    }
    
    public static IApplicationBuilder UseModules(this IApplicationBuilder app)
    {
        var modules = app.ApplicationServices.GetRequiredService<IEnumerable<IModule>>();
        
        foreach (var module in modules)
        {
            Console.WriteLine($"Configuring middleware for module: {module.Name}");
            module.ConfigureMiddleware(app);
        }
        
        return app;
    }
    
    private static List<IModule> DiscoverModules()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var moduleTypes = assembly.GetTypes()
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();
            
        var modules = new List<IModule>();
        foreach (var type in moduleTypes)
        {
            var module = (IModule)Activator.CreateInstance(type)!;
            modules.Add(module);
        }
        
        return modules;
    }
}
