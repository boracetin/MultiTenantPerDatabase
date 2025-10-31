using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultitenantPerDb.Data;
using MultitenantPerDb.Models;

namespace MultitenantPerDb.Services;

public class AuthService : IAuthService
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly IConfiguration _configuration;

    public AuthService(TenantDbContext tenantDbContext, IConfiguration configuration)
    {
        _tenantDbContext = tenantDbContext;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // Kullanıcıyı username'e göre bul
        var user = await _tenantDbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null)
        {
            return null;
        }

        // Gerçek projede BCrypt veya başka bir hash algoritması kullanılmalı
        // Şimdilik basit kontrol yapıyoruz
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        // JWT token oluştur
        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            TenantId = user.TenantId,
            ExpiresAt = expiresAt
        };
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var issuer = jwtSettings["Issuer"] ?? "MultitenantPerDb";
        var audience = jwtSettings["Audience"] ?? "MultitenantPerDbUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("TenantId", user.TenantId.ToString()), // TenantId'yi claim olarak ekliyoruz
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        // Demo için basit karşılaştırma
        // Gerçek projede BCrypt.Net-Next kullanın: BCrypt.Verify(password, passwordHash)
        return password == "123456"; // Demo password
    }
}
