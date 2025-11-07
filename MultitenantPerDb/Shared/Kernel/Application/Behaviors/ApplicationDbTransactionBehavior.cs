using MediatR;
using MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Application.Abstractions;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Shared.Kernel.Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatic transaction management with ApplicationDbContext
/// Wraps commands in database transactions (queries are excluded)
/// Used for tenant-specific operations (Products, Users, etc.)
/// Implements ICanAccessUnitOfWork as infrastructure component managing transactions
/// </summary>
public class ApplicationDbTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, ICanAccessUnitOfWork
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<ApplicationDbTransactionBehavior<TRequest, TResponse>> _logger;

    public ApplicationDbTransactionBehavior(
        IUnitOfWork<ApplicationDbContext> unitOfWork, 
        ILogger<ApplicationDbTransactionBehavior<TRequest, TResponse>> logger)
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
            _logger.LogInformation("[APP-DB NO TRANSACTION] {RequestName} - Skipping transaction (no ITransactionalCommand)", requestName);
            return await next();
        }

        _logger.LogInformation("[APP-DB TRANSACTION START] {RequestName}", requestName);

        // Transaction başlat
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Handler'ı çalıştır (business logic)
            var response = await next();

            // Transaction'ı commit et (SaveChanges + Commit)
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("[APP-DB TRANSACTION COMMIT] {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            // Hata olursa transaction'ı rollback et
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            
            _logger.LogError(ex, "[APP-DB TRANSACTION ROLLBACK] {RequestName} - Error: {ErrorMessage}", requestName, ex.Message);
            throw;
        }
    }

    private static bool IsTransactionalCommand(TRequest request)
    {
        // Check if command implements IApplicationDbTransactionalCommand
        return request is IApplicationDbTransactionalCommand;
    }
}
