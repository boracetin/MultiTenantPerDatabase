using MediatR;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Main.Identity.Application.Features.Users.GetUserByUsername;

/// <summary>
/// Query to get user by username
/// </summary>
public record GetUserByUsernameQuery(string Username) : IRequest<UserDto?>;
