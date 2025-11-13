namespace MultitenantPerDb.Core.Application.Interfaces;

/// <summary>
/// Factory interface for creating application loggers
/// </summary>
public interface ILoggerFactory
{
    /// <summary>
    /// Creates a logger for the specified type
    /// </summary>
    IAppLogger<T> CreateLogger<T>();
}

/// <summary>
/// Application logger interface
/// </summary>
public interface IAppLogger<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogTrace(string message, params object[] args);
    void LogCritical(string message, params object[] args);
    void LogCritical(Exception exception, string message, params object[] args);
}
