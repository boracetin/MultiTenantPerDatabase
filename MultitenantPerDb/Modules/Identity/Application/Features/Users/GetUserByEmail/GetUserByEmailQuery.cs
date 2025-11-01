using MediatR;
using MultitenantPerDb.Modules.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Users.GetUserByEmail;

/// <summary>
/// Query to get user by email
/// </summary>
public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;
