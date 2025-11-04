using MultitenantPerDb.Modules.Tenancy.Domain.Entities;

namespace MultitenantPerDb.Modules.Tenancy.Application.Services;

/// <summary>
/// Tenant service interface for business operations
/// </summary>
public interface ITenantService
{
    #region Query Methods
    
    Task<Tenant?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
    
    #endregion

    #region Command Methods
    
    Task<Tenant> CreateTenantAsync(
        string name, 
        string subdomain, 
        string connectionString,
        CancellationToken cancellationToken = default);
    
    Task<bool> UpdateTenantAsync(
        int tenantId,
        string? name = null,
        string? subdomain = null,
        string? connectionString = null,
        CancellationToken cancellationToken = default);
    
    Task<bool> ActivateTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    
    #endregion

    #region Validation Methods
    
    Task<bool> IsTenantNameAvailableAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsTenantSubdomainAvailableAsync(string subdomain, CancellationToken cancellationToken = default);
    
    #endregion
}
