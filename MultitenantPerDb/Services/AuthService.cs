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

        // Kullanıcının tenant'ını bul ve migration çalıştır
        var tenant = await _tenantDbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == user.TenantId && t.IsActive);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant bulunamadı: {user.TenantId}");
        }

        // Tenant database'inde migration çalıştır
        await RunTenantMigrationsAsync(tenant);

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

    /// <summary>
    /// Tenant database'inde pending migration'ları çalıştırır
    /// </summary>
    private async Task RunTenantMigrationsAsync(Tenant tenant)
    {
        try
        {
            _logger.LogInformation("Tenant database migration kontrolü: TenantId={TenantId}, Database={Database}", 
                tenant.Id, tenant.Name);

            // Tenant'ın connection string'i ile ApplicationDbContext oluştur
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(tenant.ConnectionString);

            await using var context = new ApplicationDbContext(optionsBuilder.Options);

            // Pending migration'ları kontrol et
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Pending migration'lar bulundu: {Count} adet. Migration çalıştırılıyor...", 
                    pendingMigrations.Count());
                
                // Migration'ları uygula
                await context.Database.MigrateAsync();
                
                _logger.LogInformation("Migration başarıyla tamamlandı: TenantId={TenantId}", tenant.Id);
            }
            else
            {
                _logger.LogInformation("Pending migration yok: TenantId={TenantId}", tenant.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant database migration hatası: TenantId={TenantId}", tenant.Id);
            throw new InvalidOperationException($"Tenant database migration başarısız: {ex.Message}", ex);
        }
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
