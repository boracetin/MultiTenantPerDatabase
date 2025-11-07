using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Main.Identity.Application.Services;

namespace MultitenantPerDb.Modules.Main.Identity.Application.Features.Users.GetUserByEmail;

/// <summary>
/// Handler for GetUserByEmailQuery
/// Uses UserService for data access
/// </summary>
public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetUserByEmailQueryHandler(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
            return null;

        return _mapper.Map<UserDto>(user);
    }
}
