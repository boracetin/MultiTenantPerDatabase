using MediatR;
using System.Diagnostics;
using System.Text.Json;

namespace MultitenantPerDb.Shared.Kernel.Application.Behaviors;

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

        try
        {
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
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "[ERROR] {RequestName} - Request ID: {RequestId} - Elapsed: {ElapsedMs}ms - Error: {ErrorMessage}",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.Message
            );

            throw;
        }
    }
}
