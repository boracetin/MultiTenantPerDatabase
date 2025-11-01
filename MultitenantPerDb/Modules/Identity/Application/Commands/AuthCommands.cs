using MediatR;
using MultitenantPerDb.Modules.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Identity.Application.Commands;

/// <summary>
/// Command to login a user
/// </summary>
public record LoginCommand(string Username, string Password) : IRequest<LoginResponseDto>;

/// <summary>
/// Command to register a new user
/// </summary>
public record RegisterCommand : IRequest<UserDto>
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}

/// <summary>
/// Command to refresh token
/// </summary>
public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<LoginResponseDto>;
