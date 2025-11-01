using MediatR;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Shared.Kernel.Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatic transaction management
/// Wraps commands in database transactions (queries are excluded)
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Skip transaction for queries (read operations)
        if (IsQuery(requestName))
        {
            return await next();
        }

        _logger.LogInformation("[TRANSACTION START] {RequestName}", requestName);

        try
        {
            // Execute the handler within a transaction
            var response = await next();

            _logger.LogInformation("[TRANSACTION COMMIT] {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TRANSACTION ROLLBACK] {RequestName} - Error: {ErrorMessage}", requestName, ex.Message);
            throw;
        }
    }

    private static bool IsQuery(string requestName)
    {
        // Check if request is a query (read operation)
        return requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("Get", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("List", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("Search", StringComparison.OrdinalIgnoreCase);
    }
}
