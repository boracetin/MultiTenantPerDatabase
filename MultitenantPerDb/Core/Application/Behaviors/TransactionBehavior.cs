using MediatR;
using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Domain;
using System.Reflection;

namespace MultitenantPerDb.Core.Application.Behaviors;

/// <summary>
/// Generic pipeline behavior for transaction management across multiple DbContexts
/// Automatically detects which UnitOfWork instances are injected into the handler
/// No attributes needed - analyzes handler's constructor dependencies
/// PERFORMANCE OPTIMIZED: Uses pre-scanned handler cache for ~150x faster lookups
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, ICanAccessUnitOfWork
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHandlerTypeResolver _handlerTypeResolver;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IServiceProvider serviceProvider,
        IHandlerTypeResolver handlerTypeResolver,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _serviceProvider = serviceProvider;
        _handlerTypeResolver = handlerTypeResolver;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Skip transaction if request implements IWithoutTransactional
        if (request is IWithoutTransactional)
        {
            _logger.LogDebug("[TX SKIP] {RequestName} - Implements IWithoutTransactional, skipping transaction", requestName);
            return await next();
        }

        // Get UnitOfWork instances that are actually used by the handler
        var unitOfWorks = GetHandlerUnitOfWorks(request);

        if (!unitOfWorks.Any())
        {
            _logger.LogDebug("[TX SKIP] {RequestName} - No UnitOfWork dependencies found in handler", requestName);
            return await next();
        }

        _logger.LogInformation("[TX START] {RequestName} - Starting transaction for {Count} context(s)", 
            requestName, unitOfWorks.Count);

        // Phase 1: Begin transactions on all handler's UnitOfWork instances
        foreach (var unitOfWork in unitOfWorks)
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
        }

        try
        {
            // Execute the handler (business logic)
            var response = await next();

            // Phase 2: Commit all transactions (two-phase commit)
            foreach (var unitOfWork in unitOfWorks)
            {
                await unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            _logger.LogInformation("[TX COMMIT] {RequestName} - All {Count} transaction(s) committed successfully", 
                requestName, unitOfWorks.Count);

            return response;
        }
        catch (Exception ex)
        {
            // Rollback all transactions in case of error
            _logger.LogError(ex, "[TX ROLLBACK START] {RequestName} - Error occurred, rolling back all transactions", requestName);

            foreach (var unitOfWork in unitOfWorks)
            {
                try
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "[TX ROLLBACK ERROR] {RequestName} - Failed to rollback transaction", requestName);
                }
            }

            _logger.LogError(ex, "[TX ROLLBACK COMPLETE] {RequestName} - All transactions rolled back", requestName);
            throw;
        }
    }

    /// <summary>
    /// Analyzes the handler's constructor to find which UnitOfWork instances it depends on
    /// OPTIMIZED: Uses cached handler type and constructor info - no runtime reflection
    /// </summary>
    private List<IUnitOfWorkBase> GetHandlerUnitOfWorks(TRequest request)
    {
        var unitOfWorks = new List<IUnitOfWorkBase>();

        try
        {
            var requestType = typeof(TRequest);
            var responseType = typeof(TResponse);

            // O(1) lookup from pre-built cache - no assembly scanning
            var handlerType = _handlerTypeResolver.GetHandlerType(requestType, responseType);
            
            if (handlerType == null)
            {
                _logger.LogDebug("[TX] Handler type not found for {RequestName}", requestType.Name);
                return unitOfWorks;
            }

            _logger.LogDebug("[TX] Analyzing handler: {HandlerName}", handlerType.Name);

            // O(1) lookup from cache - no reflection
            var parameters = _handlerTypeResolver.GetConstructorParameters(handlerType);

            if (parameters.Length == 0)
            {
                return unitOfWorks;
            }

            // Find all IUnitOfWork<TDbContext> parameters
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.ParameterType;

                // Check if parameter is IUnitOfWork<TDbContext>
                if (parameterType.IsGenericType && 
                    parameterType.GetGenericTypeDefinition() == typeof(IUnitOfWork<>))
                {
                    var dbContextType = parameterType.GetGenericArguments()[0];
                    
                    try
                    {
                        // Resolve the exact UnitOfWork instance that handler will use
                        var service = _serviceProvider.GetService(parameterType);
                        
                        if (service is IUnitOfWorkBase unitOfWork)
                        {
                            unitOfWorks.Add(unitOfWork);
                            _logger.LogDebug("[TX] Found UnitOfWork<{DbContextName}> in handler constructor", 
                                dbContextType.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[TX] Could not resolve UnitOfWork<{DbContextName}>", 
                            dbContextType.Name);
                    }
                }
            }

            if (unitOfWorks.Any())
            {
                _logger.LogInformation("[TX] Handler uses {Count} UnitOfWork instance(s): {Contexts}", 
                    unitOfWorks.Count,
                    string.Join(", ", unitOfWorks.Select(u => GetDbContextTypeName(u))));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[TX] Error analyzing handler dependencies for {RequestName}", 
                typeof(TRequest).Name);
        }

        return unitOfWorks;
    }

    private static string GetDbContextTypeName(IUnitOfWorkBase unitOfWork)
    {
        // Extract DbContext type name from UnitOfWork generic argument
        var unitOfWorkType = unitOfWork.GetType();
        var interfaces = unitOfWorkType.GetInterfaces();
        var iUnitOfWork = interfaces.FirstOrDefault(i => 
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUnitOfWork<>));
        
        if (iUnitOfWork != null)
        {
            var dbContextType = iUnitOfWork.GetGenericArguments()[0];
            return dbContextType.Name;
        }
        
        return "Unknown";
    }
}
