using Microsoft.Extensions.Logging;
using MultitenantPerDb.Core.Application.Interfaces;
using IAppLoggerFactory = MultitenantPerDb.Core.Application.Interfaces.ILoggerFactory;

namespace MultitenantPerDb.Core.Infrastructure.Logging;

/// <summary>
/// Microsoft.Extensions.Logging wrapper
/// </summary>
public class MicrosoftAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public MicrosoftAppLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
        => _logger.LogInformation(message, args);

    public void LogWarning(string message, params object[] args)
        => _logger.LogWarning(message, args);

    public void LogError(string message, params object[] args)
        => _logger.LogError(message, args);

    public void LogError(Exception exception, string message, params object[] args)
        => _logger.LogError(exception, message, args);

    public void LogDebug(string message, params object[] args)
        => _logger.LogDebug(message, args);

    public void LogTrace(string message, params object[] args)
        => _logger.LogTrace(message, args);

    public void LogCritical(string message, params object[] args)
        => _logger.LogCritical(message, args);

    public void LogCritical(Exception exception, string message, params object[] args)
        => _logger.LogCritical(exception, message, args);
}

/// <summary>
/// Factory for Microsoft.Extensions.Logging
/// </summary>
public class MicrosoftLoggerFactory : IAppLoggerFactory
{
    private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

    public MicrosoftLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IAppLogger<T> CreateLogger<T>()
    {
        var logger = _loggerFactory.CreateLogger<T>();
        return new MicrosoftAppLogger<T>(logger);
    }
}
