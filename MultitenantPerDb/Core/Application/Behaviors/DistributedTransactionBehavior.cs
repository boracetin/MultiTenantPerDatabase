using MediatR;
using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Domain;

namespace MultitenantPerDb.Core.Application.Behaviors;

/// <summary>
/// Pipeline behavior for distributed transaction management across multiple DbContexts
/// Manages transactions for:
/// 1. TenancyDbContext (master DB - static, always available)
/// 2. ONE tenant-specific context (ProductsDbContext OR IdentityDbContext - runtime)
/// 
/// Maximum 2 transactions at a time, not all 3 contexts simultaneously
/// </summary>
public class DistributedTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, ICanAccessUnitOfWork
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DistributedTransactionBehavior<TRequest, TResponse>> _logger;

    public DistributedTransactionBehavior(
        IServiceProvider serviceProvider,
        ILogger<DistributedTransactionBehavior<TRequest, TResponse>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Skip transaction if command doesn't require database operations
        if (!IsDistributedTransactionalCommand(request))
        {
            _logger.LogInformation("[DISTRIBUTED-TX SKIP] {RequestName} - Not a distributed transactional command", requestName);
            return await next();
        }

        _logger.LogInformation("[DISTRIBUTED-TX START] {RequestName} - Starting distributed transaction", requestName);

        // Get only the ACTIVE UnitOfWork instances that are actually being used in this request
        // This ensures we only manage transactions for contexts that are actively injected
        var unitOfWorks = GetActiveUnitOfWorks();

        if (!unitOfWorks.Any())
        {
            _logger.LogWarning("[DISTRIBUTED-TX WARNING] {RequestName} - No UnitOfWork instances found, executing without transaction", requestName);
            return await next();
        }

        _logger.LogInformation("[DISTRIBUTED-TX] {RequestName} - Managing {Count} database context(s)", requestName, unitOfWorks.Count);

        // Phase 1: Begin transactions on all ACTIVE contexts
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

            _logger.LogInformation("[DISTRIBUTED-TX COMMIT] {RequestName} - All {Count} transaction(s) committed successfully", requestName, unitOfWorks.Count);

            return response;
        }
        catch (Exception ex)
        {
            // Rollback all transactions in case of error
            _logger.LogError(ex, "[DISTRIBUTED-TX ROLLBACK START] {RequestName} - Error occurred, rolling back all transactions", requestName);

            foreach (var unitOfWork in unitOfWorks)
            {
                try
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "[DISTRIBUTED-TX ROLLBACK ERROR] {RequestName} - Failed to rollback transaction", requestName);
                }
            }

            _logger.LogError(ex, "[DISTRIBUTED-TX ROLLBACK COMPLETE] {RequestName} - All transactions rolled back", requestName);
            throw;
        }
    }

    private List<IUnitOfWorkBase> GetActiveUnitOfWorks()
    {
        var unitOfWorks = new List<IUnitOfWorkBase>();

        // 1. TenancyDbContext - Static, always available (Master DB)
        TryAddUnitOfWork(unitOfWorks, GetTenancyDbContextType(), "TenancyDbContext");

        // 2. Try to get ONE tenant-specific context that's actually being used
        // Only the context that's injected in the current request scope will be resolved
        // ProductsDbContext OR IdentityDbContext (not both - handler uses one)
        TryAddUnitOfWork(unitOfWorks, GetProductsDbContextType(), "ProductsDbContext");
        TryAddUnitOfWork(unitOfWorks, GetIdentityDbContextType(), "IdentityDbContext");

        return unitOfWorks;
    }

    private void TryAddUnitOfWork(List<IUnitOfWorkBase> unitOfWorks, Type dbContextType, string contextName)
    {
        try
        {
            var unitOfWorkType = typeof(IUnitOfWork<>).MakeGenericType(dbContextType);
            var service = _serviceProvider.GetService(unitOfWorkType);
            
            if (service is IUnitOfWorkBase unitOfWork)
            {
                unitOfWorks.Add(unitOfWork);
                _logger.LogDebug("[DISTRIBUTED-TX] {ContextName} UnitOfWork found and added", contextName);
            }
            else
            {
                _logger.LogDebug("[DISTRIBUTED-TX] {ContextName} UnitOfWork not active in current scope", contextName);
            }
        }
        catch (Exception ex)
        {
            // Service not registered or not available in current scope - this is expected
            _logger.LogDebug(ex, "[DISTRIBUTED-TX] {ContextName} not available: {Message}", contextName, ex.Message);
        }
    }

    private static Type GetTenancyDbContextType()
    {
        return Type.GetType("MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence.TenancyDbContext, MultitenantPerDb") 
               ?? throw new InvalidOperationException("TenancyDbContext type not found");
    }

    private static Type GetProductsDbContextType()
    {
        return Type.GetType("MultitenantPerDb.Modules.Products.Infrastructure.Persistence.ProductsDbContext, MultitenantPerDb")
               ?? throw new InvalidOperationException("ProductsDbContext type not found");
    }

    private static Type GetIdentityDbContextType()
    {
        return Type.GetType("MultitenantPerDb.Modules.Identity.Infrastructure.Persistence.IdentityDbContext, MultitenantPerDb")
               ?? throw new InvalidOperationException("IdentityDbContext type not found");
    }

    private static bool IsDistributedTransactionalCommand(TRequest request)
    {
        // Check if command implements IDistributedTransactionalCommand marker interface
        return request is IDistributedTransactionalCommand;
    }
}
