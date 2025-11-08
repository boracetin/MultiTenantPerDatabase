using MediatR;
using System.Diagnostics;
using System.Text.Json;

namespace MultitenantPerDb.Core.Application.Behaviors;

/// <summary>
/// Pipeline behavior for logging all commands and queries
/// Logs request, response, and execution time
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "[START] {RequestName} - Request ID: {RequestId} - Request: {Request}",
            requestName,
            requestId,
            JsonSerializer.Serialize(request)
        );

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        _logger.LogInformation(
            "[END] {RequestName} - Request ID: {RequestId} - Elapsed: {ElapsedMs}ms",
            requestName,
            requestId,
            stopwatch.ElapsedMilliseconds
        );

        return response;
    }
}
