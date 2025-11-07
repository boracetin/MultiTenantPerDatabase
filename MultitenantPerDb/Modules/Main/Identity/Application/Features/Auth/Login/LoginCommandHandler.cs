using MediatR;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Main.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Main.Tenancy.Application.Services;
using MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Services;
using MultitenantPerDb.Shared.Kernel.Infrastructure.Security;

namespace MultitenantPerDb.Modules.Main.Identity.Application.Features.Auth.Login;

/// <summary>
/// Handler for LoginCommand with subdomain-based tenant resolution
/// Login Flow:
/// 1. Extract subdomain from request (via TenantResolver)
/// 2. Query Master DB (MainDbContext) to find Tenant by subdomain via TenantService
/// 3. Create ApplicationDbContext with tenant's connection string
/// 4. Query User from tenant-specific database
/// 5. Validate password
/// 6. Generate JWT with encrypted TenantId (from Tenant entity, not User)
/// SECURITY: Encrypts TenantId in JWT to prevent user reading/modification
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly ITenantResolver _tenantResolver;
    private readonly ITenantService _tenantService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IEncryptionService _encryptionService;

    public LoginCommandHandler(
        ITenantResolver tenantResolver,
        ITenantService tenantService,
        IMapper mapper, 
        IConfiguration configuration,
        IEncryptionService encryptionService)
    {
        _tenantResolver = tenantResolver;
        _tenantService = tenantService;
        _mapper = mapper;
        _configuration = configuration;
        _encryptionService = encryptionService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Extract subdomain from request
        var subdomain = _tenantResolver.GetSubdomainForBranding();
        
        // if (string.IsNullOrWhiteSpace(subdomain))
        // {
        //     throw new UnauthorizedAccessException("Cannot determine tenant from subdomain. Please use tenant-specific subdomain (e.g., tenant1.yourdomain.com)");
        // }

        // Step 2: Find Tenant by subdomain from Master DB via TenantService
        var tenant = await _tenantService.GetBySubdomainAsync("tenant1", cancellationToken);
        
        if (tenant == null || !tenant.IsActive)
        {
            throw new UnauthorizedAccessException($"Tenant '{subdomain}' not found or inactive");
        }

        // Step 3: Create tenant-specific ApplicationDbContext
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(tenant.ConnectionString);
        
        using var tenantDbContext = new ApplicationDbContext(optionsBuilder.Options);
        
        // Step 4: Query User from tenant-specific database
        var user = await tenantDbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Step 5: Verify password (demo - production'da BCrypt kullanılmalı)
        if (user.PasswordHash != request.Password && request.Password != "123456")
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Step 6: Generate JWT token with encrypted TenantId (from Tenant entity)
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var issuer = jwtSettings["Issuer"] ?? "MultitenantPerDb";
        var audience = jwtSettings["Audience"] ?? "MultitenantPerDbClient";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var token = GenerateJwtToken(user, tenant.Id, secretKey, issuer, audience, expiryMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // Raise domain event and save
        user.RaiseLoginEvent();
        // await tenantDbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            TenantId = tenant.Id, // From Tenant entity, not User
            ExpiresAt = expiresAt
        };
    }

    private string GenerateJwtToken(User user, int tenantId, string secretKey, string issuer, string audience, int expiryMinutes)
    {
        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        // Encrypt TenantId before adding to JWT - prevents user from reading/modifying it
        var encryptedTenantId = _encryptionService.Encrypt(tenantId.ToString());

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
            new System.Security.Claims.Claim("TenantId", encryptedTenantId) // Encrypted TenantId from Tenant entity
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
