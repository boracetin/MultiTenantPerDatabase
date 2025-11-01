using MediatR;
using MultitenantPerDb.Modules.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Users.GetUserByUsername;

/// <summary>
/// Query to get user by username
/// </summary>
public record GetUserByUsernameQuery(string Username) : IRequest<UserDto?>;
