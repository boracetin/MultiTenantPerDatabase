using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Main.Identity.Application.Services;

namespace MultitenantPerDb.Modules.Main.Identity.Application.Features.Users.GetUserByUsername;

/// <summary>
/// Handler for GetUserByUsernameQuery
/// Uses UserService for data access
/// </summary>
public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, UserDto?>
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetUserByUsernameQueryHandler(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null)
            return null;

        return _mapper.Map<UserDto>(user);
    }
}
