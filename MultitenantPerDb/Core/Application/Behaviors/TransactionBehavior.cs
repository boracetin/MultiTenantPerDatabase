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
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, ICanAccessUnitOfWork
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IServiceProvider serviceProvider,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

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
    /// Only resolves those specific UnitOfWork instances from DI container
    /// </summary>
    private List<IUnitOfWorkBase> GetHandlerUnitOfWorks(TRequest request)
    {
        var unitOfWorks = new List<IUnitOfWorkBase>();

        try
        {
            // Find the handler type for this request
            var handlerType = FindHandlerType(request);
            
            if (handlerType == null)
            {
                _logger.LogDebug("[TX] Handler type not found for {RequestName}", typeof(TRequest).Name);
                return unitOfWorks;
            }

            _logger.LogDebug("[TX] Analyzing handler: {HandlerName}", handlerType.Name);

            // Get handler's constructor parameters
            var constructors = handlerType.GetConstructors();
            var constructor = constructors.FirstOrDefault();

            if (constructor == null)
            {
                return unitOfWorks;
            }

            var parameters = constructor.GetParameters();

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

    /// <summary>
    /// Finds the handler type that handles this request
    /// Searches for IRequestHandler<TRequest, TResponse> implementation
    /// </summary>
    private Type? FindHandlerType(TRequest request)
    {
        var requestType = typeof(TRequest);
        var responseType = typeof(TResponse);
        var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        // Search in all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                
                foreach (var type in types)
                {
                    if (type.IsClass && !type.IsAbstract)
                    {
                        var interfaces = type.GetInterfaces();
                        
                        if (interfaces.Any(i => i == handlerInterfaceType))
                        {
                            return type;
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Some assemblies might fail to load - continue with others
                _logger.LogDebug(ex, "[TX] Could not load types from assembly {AssemblyName}", 
                    assembly.FullName);
            }
        }

        return null;
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
