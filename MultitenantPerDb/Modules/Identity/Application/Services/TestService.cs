using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Application;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Identity.Application.Services;

/// <summary>
/// TEST: This service should trigger MTDB003 warning!
/// - Name ends with "Service" ✓
/// - Has IUnitOfWork<T> in constructor ✓
/// - Does NOT inherit from BaseService ✗
/// Expected: MTDB003 warning
/// </summary>
public class TestService
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;

    public TestService(IUnitOfWork<ApplicationDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets Repository<User> from UnitOfWork
    /// UnitOfWork ensures same context instance is used for all repositories
    /// </summary>
    private IRepository<User> GetRepository()
    {
        return _unitOfWork.GetRepository<User>();
    }


}
