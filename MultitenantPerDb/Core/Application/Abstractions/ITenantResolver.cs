namespace MultitenantPerDb.Core.Application.Abstractions;

/// <summary>
/// Interface for resolving current tenant from HTTP context or background jobs
/// SECURITY: TenantId comes only from secure sources (JWT claims or explicit set)
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Gets TenantId from secure sources only (JWT claims or explicit set)
    /// </summary>
    string? TenantId { get; }
    
    /// <summary>
    /// Explicitly set tenant ID (for background jobs and internal operations only)
    /// </summary>
    void SetTenant(string tenantId);
    
    /// <summary>
    /// Clear explicitly set tenant
    /// </summary>
    void ClearTenant();
    
    /// <summary>
    /// Check if tenant is resolved
    /// </summary>
    bool HasTenant();
}
