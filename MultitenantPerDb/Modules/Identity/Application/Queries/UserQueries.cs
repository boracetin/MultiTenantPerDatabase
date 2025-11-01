using MediatR;
using MultitenantPerDb.Modules.Identity.Application.DTOs;

namespace MultitenantPerDb.Modules.Identity.Application.Queries;

/// <summary>
/// Query to get user by username
/// </summary>
public record GetUserByUsernameQuery(string Username) : IRequest<UserDto?>;

/// <summary>
/// Query to get user by email
/// </summary>
public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;

/// <summary>
/// Query to get user by ID
/// </summary>
public record GetUserByIdQuery(int Id) : IRequest<UserDto?>;

/// <summary>
/// Query to get all users (for admin purposes)
/// </summary>
public record GetAllUsersQuery : IRequest<List<UserDto>>;
