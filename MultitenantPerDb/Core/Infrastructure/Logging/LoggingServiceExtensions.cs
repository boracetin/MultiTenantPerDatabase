using Microsoft.Extensions.DependencyInjection;
using MultitenantPerDb.Core.Application.Interfaces;

namespace MultitenantPerDb.Core.Infrastructure.Logging;

public static class LoggingServiceExtensions
{
    /// <summary>
    /// Adds Microsoft.Extensions.Logging as the logger provider
    /// </summary>
    public static IServiceCollection AddMicrosoftLogger(this IServiceCollection services)
    {
        services.AddSingleton<Application.Interfaces.ILoggerFactory, MicrosoftLoggerFactory>();
        return services;
    }

    /// <summary>
    /// Adds logger based on configuration setting
    /// Currently only Microsoft.Extensions.Logging is supported
    /// To add Serilog: install Serilog.AspNetCore package and implement SerilogLoggerFactory
    /// </summary>
    public static IServiceCollection AddConfiguredLogger(
        this IServiceCollection services, 
        string loggerProvider)
    {
        // Currently only Microsoft logging is implemented
        // To add Serilog support:
        // 1. Install: dotnet add package Serilog.AspNetCore
        // 2. Create SerilogLoggerFactory implementing ILoggerFactory
        // 3. Add "serilog" case below
        
        return services.AddMicrosoftLogger();
    }
}
