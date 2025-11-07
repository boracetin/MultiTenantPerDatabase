using MediatR;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;
using MultitenantPerDb.Shared.Kernel.Application.Abstractions;

namespace MultitenantPerDb.Modules.Main.Identity.Application.Features.Auth.Login;

/// <summary>
/// Command to login a user
/// Uses MainDbContext to query tenant information
/// Implements IMainDbTransactionalCommand to enable database transaction with MainDbContext
/// </summary>
public record LoginCommand(string Username, string Password) : IRequest<LoginResponseDto>, IMainDbTransactionalCommand;
