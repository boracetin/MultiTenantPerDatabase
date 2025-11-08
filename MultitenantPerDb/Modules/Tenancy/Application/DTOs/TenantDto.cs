namespace MultitenantPerDb.Modules.Tenancy.Application.DTOs;

/// <summary>
/// Data Transfer Object for Tenant
/// Used for API responses and queries
/// </summary>
public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public string? DisplayName { get; set; }
    public string? LogoUrl { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? CustomCss { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
