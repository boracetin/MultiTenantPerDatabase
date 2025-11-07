using MediatR;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Main.Identity.Application.Features.Users.GetUserByEmail;

/// <summary>
/// Query to get user by email
/// </summary>
public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;
