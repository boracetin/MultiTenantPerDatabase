namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

/// <summary>
/// Interface for resolving current tenant from HTTP context or background jobs
/// </summary>
public interface ITenantResolver
{
    string? TenantId { get; }
    void SetTenant(string tenantId);
    void ClearTenant();
    bool HasTenant();
}
