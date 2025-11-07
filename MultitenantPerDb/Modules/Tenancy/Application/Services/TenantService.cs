using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Application;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Tenancy.Application.Services;

/// <summary>
/// Tenant service implementation
/// Uses IUnitOfWork<MainDbContext> to access Repository<Tenant> for data access
/// UnitOfWork manages the MainDbContext and ensures single instance per request
/// Inherits from BaseService to enforce ICanAccessUnitOfWork constraint (checked by MTDB003 analyzer)
/// </summary>
public class TenantService : BaseService, ITenantService
{
    private readonly IUnitOfWork<MainDbContext> _unitOfWork;

    public TenantService(IUnitOfWork<MainDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets Repository<Tenant, int> from UnitOfWork
    /// UnitOfWork ensures same context instance is used for all repositories
    /// </summary>
    private IRepository<Tenant, int> GetRepository()
    {
        return _unitOfWork.GetRepository<Tenant, int>();
    }

    #region Query Methods

    public async Task<Tenant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.FirstOrDefaultAsync(
            t => t.Name == name,
            asNoTracking: true,
            cancellationToken);
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.FirstOrDefaultAsync(
            t => t.Subdomain == subdomain,
            asNoTracking: true,
            cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.FindAsync(
            t => t.IsActive,
            asNoTracking: true,
            cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.GetAllAsync(asNoTracking: true, cancellationToken);
    }

    #endregion

    #region Command Methods

    public async Task<Tenant> CreateTenantAsync(
        string name,
        string subdomain,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();

        // ✅ Business validation
        var nameExists = await repository.AnyAsync(
            t => t.Name == name,
            cancellationToken);

        if (nameExists)
            throw new InvalidOperationException($"Tenant name '{name}' already exists");

        var subdomainExists = await repository.AnyAsync(
            t => t.Subdomain == subdomain,
            cancellationToken);

        if (subdomainExists)
            throw new InvalidOperationException($"Subdomain '{subdomain}' already exists");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        // ✅ Aggregate Root factory method
        var tenant = Tenant.Create(name, subdomain, connectionString);

        // ✅ Save via repository
        await repository.AddAsync(tenant, cancellationToken);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return tenant;
    }

    public async Task<bool> UpdateTenantAsync(
        int tenantId,
        string? name = null,
        string? subdomain = null,
        string? connectionString = null,
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var tenant = await repository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");

        // ✅ Business validation for name uniqueness
        if (!string.IsNullOrEmpty(name) && name != tenant.Name)
        {
            var nameExists = await repository.AnyAsync(
                t => t.Name == name && t.Id != tenantId,
                cancellationToken);

            if (nameExists)
                throw new InvalidOperationException($"Tenant name '{name}' is already in use");
        }

        // Update details
        var finalName = name ?? tenant.Name;
        var finalSubdomain = subdomain ?? tenant.Subdomain ?? string.Empty;
        var finalConnectionString = connectionString ?? tenant.ConnectionString;

        // ✅ Aggregate Root business method
        tenant.UpdateDetails(finalName, finalSubdomain, finalConnectionString);

        repository.Update(tenant);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    public async Task<bool> ActivateTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var tenant = await repository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");

        // ✅ Aggregate Root business method
        tenant.Activate();

        repository.Update(tenant);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    public async Task<bool> DeactivateTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var tenant = await repository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");

        // ✅ Aggregate Root business method
        tenant.Deactivate();

        repository.Update(tenant);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    public async Task<bool> DeleteTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var tenant = await repository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");

        // ✅ Business rule: Can't delete active tenants (must deactivate first)
        if (tenant.IsActive)
            throw new InvalidOperationException("Cannot delete an active tenant. Deactivate the tenant first.");

        // ✅ Soft delete - marks entity as deleted
        repository.Delete(tenant);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    #endregion

    #region Validation Methods

    public async Task<bool> IsTenantNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var exists = await repository.AnyAsync(
            t => t.Name == name,
            cancellationToken);

        return !exists;
    }

    public async Task<bool> IsTenantSubdomainAvailableAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var exists = await repository.AnyAsync(
            t => t.Subdomain == subdomain,
            cancellationToken);

        return !exists;
    }

    #endregion
}
