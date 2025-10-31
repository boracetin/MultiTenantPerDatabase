namespace MultitenantPerDb.Models;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
