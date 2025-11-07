using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Main.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Main.Tenancy.Domain.Entities;

namespace MultitenantPerDb.Modules.Main.Identity.Application.Services;

/// <summary>
/// Authentication service implementation (DEPRECATED)
/// Use LoginCommandHandler instead
/// </summary>
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // NOTE: This service is DEPRECATED - Use LoginCommandHandler with subdomain-based auth instead
        await Task.CompletedTask; // Suppress async warning
        throw new NotImplementedException("Use LoginCommandHandler with subdomain-based authentication instead");
    }

    public string GenerateJwtToken(int userId, string username, string email, int tenantId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var issuer = jwtSettings["Issuer"] ?? "MultitenantPerDb";
        var audience = jwtSettings["Audience"] ?? "MultitenantPerDbUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim("TenantId", tenantId.ToString()),
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

    private async Task RunTenantMigrationsAsync(Tenant tenant)
    {
        try
        {
            _logger.LogInformation("Tenant database migration kontrolü: TenantId={TenantId}", tenant.Id);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(tenant.ConnectionString);

            await using var context = new ApplicationDbContext(optionsBuilder.Options);

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Pending migration'lar bulundu: {Count} adet", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                _logger.LogInformation("Migration başarıyla tamamlandı: TenantId={TenantId}", tenant.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant database migration hatası: TenantId={TenantId}", tenant.Id);
            throw new InvalidOperationException($"Tenant database migration başarısız: {ex.Message}", ex);
        }
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        // Demo için basit karşılaştırma
        // Production'da BCrypt.Verify(password, passwordHash) kullanılmalı
        return password == "123456";
    }
}
