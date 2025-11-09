using MediatR;
using Microsoft.AspNetCore.Identity;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Application.Services;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;
using MultitenantPerDb.Core.Infrastructure.Security;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Application.Interfaces;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly ITenantResolver _tenantResolver;
    private readonly ITenantService _tenantService;
    private readonly IConfiguration _configuration;
    private readonly IEncryptionService _encryptionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantDbContextFactory<ApplicationIdentityDbContext> _identityDbContextFactory;

    public LoginCommandHandler(
        ITenantResolver tenantResolver,
        ITenantService tenantService,
        IConfiguration configuration,
        IEncryptionService encryptionService,
        ITenantDbContextFactory<ApplicationIdentityDbContext> identityDbContextFactory,
        ICurrentUserService currentUserService)
    {
        _tenantResolver = tenantResolver;
        _tenantService = tenantService;
        _configuration = configuration;
        _encryptionService = encryptionService;
        _identityDbContextFactory = identityDbContextFactory;
        _currentUserService = currentUserService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var subdomain = _currentUserService.TenantName ?? "tenant1";
        var tenant = await _tenantService.GetBySubdomainAsync(subdomain, cancellationToken);
        
        if (tenant == null || !tenant.IsActive)
        {
            throw new UnauthorizedAccessException("Tenant not found or inactive");
        }

        // Temporarily set tenant ID for login process
        // This allows the factory to create the tenant-specific ApplicationIdentityDbContext
        _tenantResolver.SetTenant(tenant.Id.ToString());
        
        try
        {
            var tenantDbContext = _identityDbContextFactory.CreateDbContext();
            var userStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<IdentityUser>(tenantDbContext);
            var userManager = new UserManager<IdentityUser>(
                userStore, null, new PasswordHasher<IdentityUser>(), null, null, null, null, null, null);

            var user = await userManager.FindByEmailAsync(request.Username);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            var issuer = jwtSettings["Issuer"] ?? "MultitenantPerDb";
            var audience = jwtSettings["Audience"] ?? "MultitenantPerDbClient";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var token = GenerateJwtToken(user, tenant.Id, secretKey, issuer, audience, expiryMinutes);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            return new LoginResponseDto
            {
                Token = token,
                Username = user.UserName ?? string.Empty,
                TenantId = tenant.Id,
                ExpiresAt = expiresAt
            };
        }
        finally
        {
            // Clear the temporary tenant setting
            _tenantResolver.ClearTenant();
        }
    }

    private string GenerateJwtToken(IdentityUser user, int tenantId, string secretKey, string issuer, string audience, int expiryMinutes)
    {
        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var encryptedTenantId = _encryptionService.Encrypt(tenantId.ToString());

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.UserName ?? string.Empty),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty),
            new System.Security.Claims.Claim("TenantId", encryptedTenantId)
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
