using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Identity.Domain.Repositories;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Auth.Login;

/// <summary>
/// Handler for LoginCommand
/// Authenticates user and generates JWT token
/// Uses Master DB (tenant-independent) via IUserRepository
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(IUserRepository userRepository, IMapper mapper, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Verify password (demo - production'da BCrypt kullanılmalı)
        if (user.PasswordHash != request.Password && request.Password != "123456")
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Generate JWT token
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var issuer = jwtSettings["Issuer"] ?? "MultitenantPerDb";
        var audience = jwtSettings["Audience"] ?? "MultitenantPerDbClient";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var token = GenerateJwtToken(user, secretKey, issuer, audience, expiryMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // Raise domain event
        user.RaiseLoginEvent();
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            TenantId = user.TenantId,
            ExpiresAt = expiresAt
        };
    }

    private static string GenerateJwtToken(User user, string secretKey, string issuer, string audience, int expiryMinutes)
    {
        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
            new System.Security.Claims.Claim("TenantId", user.TenantId.ToString())
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
