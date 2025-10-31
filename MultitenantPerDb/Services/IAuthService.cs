using MultitenantPerDb.Models;

namespace MultitenantPerDb.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    string GenerateJwtToken(User user);
}
