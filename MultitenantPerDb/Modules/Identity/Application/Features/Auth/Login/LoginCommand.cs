using MediatR;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Core.Application.Abstractions;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Auth.Login;

/// <summary>
/// Command to login a user
/// Uses TenancyDbContext to query tenant information
/// </summary>
public record LoginCommand(string Username, string Password) : IRequest<LoginResponseDto>;
