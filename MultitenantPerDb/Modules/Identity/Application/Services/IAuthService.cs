using MultitenantPerDb.Modules.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Identity.Application.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    string GenerateJwtToken(int userId, string username, string email, int tenantId);
}
