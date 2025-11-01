using MediatR;
using MultitenantPerDb.Modules.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Auth.Login;

/// <summary>
/// Command to login a user
/// </summary>
public record LoginCommand(string Username, string Password) : IRequest<LoginResponseDto>;
