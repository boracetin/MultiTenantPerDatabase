using MediatR;
using System.ComponentModel.DataAnnotations;

namespace MultitenantPerDb.Shared.Kernel.Application.Behaviors;

/// <summary>
/// Pipeline behavior for global exception handling
/// Catches all exceptions and converts them to appropriate responses
/// Should be registered FIRST in pipeline (outermost behavior)
/// </summary>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            // Execute the next behavior/handler in pipeline
            return await next();
        }
        catch (ValidationException validationEx)
        {
            // Validation errors - 400 Bad Request
            _logger.LogWarning(
                validationEx,
                "[VALIDATION ERROR] {RequestName} - {Message}",
                typeof(TRequest).Name,
                validationEx.Message
            );
            throw; // Re-throw to be handled by ValidationBehavior or API layer
        }
        catch (UnauthorizedAccessException unauthorizedEx)
        {
            // Authorization errors - 401/403
            _logger.LogWarning(
                unauthorizedEx,
                "[UNAUTHORIZED] {RequestName} - {Message}",
                typeof(TRequest).Name,
                unauthorizedEx.Message
            );
            throw; // Re-throw to be handled by API middleware
        }
        catch (KeyNotFoundException notFoundEx)
        {
            // Not found errors - 404
            _logger.LogWarning(
                notFoundEx,
                "[NOT FOUND] {RequestName} - {Message}",
                typeof(TRequest).Name,
                notFoundEx.Message
            );
            throw; // Re-throw to be handled by API middleware
        }
        catch (InvalidOperationException invalidOpEx)
        {
            // Business rule violations - 400/422
            _logger.LogWarning(
                invalidOpEx,
                "[BUSINESS RULE VIOLATION] {RequestName} - {Message}",
                typeof(TRequest).Name,
                invalidOpEx.Message
            );
            throw; // Re-throw to be handled by API middleware
        }
        catch (Exception ex)
        {
            // Unexpected errors - 500 Internal Server Error
            _logger.LogError(
                ex,
                "[UNHANDLED ERROR] {RequestName} - {Message}",
                typeof(TRequest).Name,
                ex.Message
            );
            
            // In production, don't expose internal error details to client
            throw new InvalidOperationException(
                "An unexpected error occurred while processing your request. Please try again later.",
                ex
            );
        }
    }
}
