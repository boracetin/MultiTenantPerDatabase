using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultitenantPerDb.Application.DTOs;
using MultitenantPerDb.Domain.Entities;
using MultitenantPerDb.Infrastructure.Persistence;

namespace MultitenantPerDb.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        TenantDbContext tenantDbContext,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _tenantDbContext = tenantDbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // Kullanıcıyı username'e göre bul
        var user = await _tenantDbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null)
        {
            return null;
        }

        // Password verification (demo - production'da BCrypt kullanılmalı)
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Kullanıcının tenant'ını bul ve migration çalıştır
        var tenant = await _tenantDbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == user.TenantId && t.IsActive);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant bulunamadı: {user.TenantId}");
        }

        // Tenant database'inde migration çalıştır
        await RunTenantMigrationsAsync(tenant);

        // Domain event
        user.RaiseLoginEvent();

        // JWT token oluştur
        var token = GenerateJwtToken(user.Id, user.Username, user.Email, user.TenantId);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            TenantId = user.TenantId,
            ExpiresAt = expiresAt
        };
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
