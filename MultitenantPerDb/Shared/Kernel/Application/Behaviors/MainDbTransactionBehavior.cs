using MediatR;
using MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Application.Abstractions;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Shared.Kernel.Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatic transaction management with MainDbContext
/// Wraps commands in database transactions (queries are excluded)
/// Used for tenant management operations (Login, Tenant CRUD, etc.)
/// Implements ICanAccessUnitOfWork as infrastructure component managing transactions
/// </summary>
public class MainDbTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, ICanAccessUnitOfWork
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork<MainDbContext> _unitOfWork;
    private readonly ILogger<MainDbTransactionBehavior<TRequest, TResponse>> _logger;

    public MainDbTransactionBehavior(
        IUnitOfWork<MainDbContext> unitOfWork, 
        ILogger<MainDbTransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Skip transaction if command doesn't require database operations
        if (!IsTransactionalCommand(request))
        {
            _logger.LogInformation("[MAIN-DB NO TRANSACTION] {RequestName} - Skipping transaction (no ITransactionalCommand)", requestName);
            return await next();
        }

        _logger.LogInformation("[MAIN-DB TRANSACTION START] {RequestName}", requestName);

        // Transaction başlat
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Handler'ı çalıştır (business logic)
            var response = await next();

            // Transaction'ı commit et (SaveChanges + Commit)
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("[MAIN-DB TRANSACTION COMMIT] {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            // Hata olursa transaction'ı rollback et
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            
            _logger.LogError(ex, "[MAIN-DB TRANSACTION ROLLBACK] {RequestName} - Error: {ErrorMessage}", requestName, ex.Message);
            throw;
        }
    }

    private static bool IsTransactionalCommand(TRequest request)
    {
        // Check if command implements IMainDbTransactionalCommand
        return request is IMainDbTransactionalCommand;
    }
}
