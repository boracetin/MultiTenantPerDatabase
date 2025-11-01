using MultitenantPerDb.Shared.Kernel.Infrastructure;
using MultitenantPerDb.Modules.Products.Domain.Services;
using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;
using MultitenantPerDb.Shared.Kernel.Infrastructure.Services.Implementations;

namespace MultitenantPerDb.Shared.Kernel.Application;

/// <summary>
/// Shared Services Registration
/// Tüm modüller tarafından kullanılabilecek service'ler burada kayıt edilir
/// </summary>
public static class SharedServicesRegistration
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Infrastructure Services
        // Production'da real implementation, Development'ta fake
        if (configuration.GetValue<bool>("UseFakeServices", true))
        {
            services.AddScoped<IEmailService, FakeEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, EmailService>();
        }

        // Domain Services - Stateless, thread-safe
        services.AddSingleton<IPriceCalculationService, PriceCalculationService>();

        return services;
    }
}
