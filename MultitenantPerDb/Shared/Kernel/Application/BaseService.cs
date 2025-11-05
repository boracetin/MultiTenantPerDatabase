using Microsoft.EntityFrameworkCore;

namespace MultitenantPerDb.Shared.Kernel.Application;

/// <summary>
/// Base service class with protected UnitOfWork access
/// Only services inheriting from this class can access UnitOfWork
/// Enforces ICanAccessUnitOfWork constraint at compile-time
/// </summary>
/// <typeparam name="TDbContext">The DbContext type</typeparam>
public abstract class BaseService<TDbContext> : Domain.ICanAccessUnitOfWork
    where TDbContext : DbContext
{
    protected readonly Domain.IUnitOfWork<TDbContext> _unitOfWork;

    protected BaseService(Domain.IUnitOfWork<TDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }
}
