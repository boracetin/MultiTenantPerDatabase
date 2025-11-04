using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Application.Services;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Auth.Register;

/// <summary>
/// Handler for RegisterCommand
/// Creates a new user account using UserService
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserDto>
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // UserService handles all validations and business logic
        var user = await _userService.CreateUserAsync(
            username: request.Username,
            email: request.Email,
            password: request.Password, // UserService should hash this in production
            cancellationToken: cancellationToken
        );

        return _mapper.Map<UserDto>(user);
    }
}
