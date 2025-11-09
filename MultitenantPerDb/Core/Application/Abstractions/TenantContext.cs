namespace MultitenantPerDb.Core.Application.Abstractions;

/// <summary>
/// Background işlemler için tenant context wrapper
/// Hangfire, BackgroundService gibi HttpContext olmayan yerlerde kullanılır
/// </summary>
public class TenantContext
{
    public string TenantId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
