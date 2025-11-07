using MediatR;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Main.Identity.Application.Features.Auth.Register;

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
