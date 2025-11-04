using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Tenancy.Application.Services;

/// <summary>
/// Tenant service implementation
/// Uses MainDbContext directly for data access (Master DB)
/// Note: Different from other services - uses MainDbContext, not ApplicationDbContext via factory
/// MainDbContext manages all tenants, not tenant-specific data
/// </summary>
public class TenantService : ITenantService
{
    private readonly MainDbContext _mainDbContext;
    private IRepository<Tenant>? _tenantRepository;

    public TenantService(MainDbContext mainDbContext)
    {
        _mainDbContext = mainDbContext;
    }

    /// <summary>
    /// Gets Repository<Tenant> using MainDbContext
    /// MainDbContext is injected directly, not via factory
    /// </summary>
    private IRepository<Tenant> GetRepository()
    {
        if (_tenantRepository == null)
        {
            _tenantRepository = new Repository<Tenant>(_mainDbContext);
        }
        return _tenantRepository;
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
        await _mainDbContext.SaveChangesAsync(cancellationToken);

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
        await _mainDbContext.SaveChangesAsync(cancellationToken);

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
        await _mainDbContext.SaveChangesAsync(cancellationToken);

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
        await _mainDbContext.SaveChangesAsync(cancellationToken);

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

        // ✅ Soft delete if supported, otherwise hard delete
        repository.SoftDelete(tenant);
        await _mainDbContext.SaveChangesAsync(cancellationToken);

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
