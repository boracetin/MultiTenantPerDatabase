namespace MultitenantPerDb.Modules.Identity.Application.DTOs;

public record LoginRequestDto
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record LoginResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public int TenantId { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public record UserDto
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    // TenantId removed - implicit in tenant-specific database
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
