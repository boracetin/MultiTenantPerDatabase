namespace MultitenantPerDb.Services;

public interface ITenantResolver
{
    string? TenantId { get; }
    void SetTenant(string tenantId);
    void ClearTenant();
    bool HasTenant();
}
