using MultitenantPerDb.Application.DTOs;

namespace MultitenantPerDb.Application.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    string GenerateJwtToken(int userId, string username, string email, int tenantId);
}
