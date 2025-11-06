using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace MultitenantPerDb.Shared.Kernel.Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatic validation using FluentValidation
/// Validates Commands/Queries (DTOs) before they reach handlers
/// Throws ValidationException with detailed error messages
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // If no validators registered, skip validation
        if (!_validators.Any())
        {
            _logger.LogDebug("[VALIDATION SKIPPED] {RequestName} - No validators registered", requestName);
            return await next();
        }

        _logger.LogDebug("[VALIDATION START] {RequestName} - Validators: {ValidatorCount}", 
            requestName, _validators.Count());

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel for performance
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        // Collect all failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If validation failed, throw ValidationException with detailed errors
        if (failures.Any())
        {
            _logger.LogWarning(
                "[VALIDATION FAILED] {RequestName} - Errors: {ErrorCount} - Details: {Errors}",
                requestName,
                failures.Count,
                string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"))
            );

            throw new ValidationException(failures);
        }

        _logger.LogDebug("[VALIDATION SUCCESS] {RequestName}", requestName);

        // Continue to next behavior or handler
        return await next();
    }
}
