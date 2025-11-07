using MultitenantPerDb.Shared.Kernel.Infrastructure;
using MultitenantPerDb.Modules.Application.Products.Domain.Services;
using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;
using MultitenantPerDb.Shared.Kernel.Infrastructure.Services.Implementations;

namespace MultitenantPerDb.Shared.Kernel.Application;

/// <summary>
/// Shared Services Registration
/// Generic infrastructure services - reusable across all modules
/// </summary>
public static class SharedServicesRegistration
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Check if we should use fake services (for development/testing)
        var useFakeServices = configuration.GetValue<bool>("UseFakeServices", true);

        // ===== INFRASTRUCTURE SERVICES (GENERIC) =====
        // Pure infrastructure - no domain knowledge
        
        // Email Service
        if (useFakeServices)
        {
            services.AddScoped<IEmailService, FakeEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, EmailService>();
        }

        // SMS Service
        if (useFakeServices)
        {
            services.AddScoped<ISmsService, FakeSmsService>();
        }
        else
        {
            services.AddScoped<ISmsService, SmsService>();
        }

        // File Storage Service
        if (useFakeServices)
        {
            services.AddScoped<IFileStorageService, FakeFileStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, FileStorageService>();
        }

        // HTTP Client Service (Generic API calls)
        services.AddHttpClient(); // Required for HttpClientService
        if (useFakeServices)
        {
            services.AddScoped<IHttpClientService, FakeHttpClientService>();
        }
        else
        {
            services.AddScoped<IHttpClientService, HttpClientService>();
        }

        // ===== DOMAIN SERVICES (SHARED) =====
        // Stateless business logic - thread-safe
        services.AddSingleton<IPriceCalculationService, PriceCalculationService>();

        return services;
    }
}

