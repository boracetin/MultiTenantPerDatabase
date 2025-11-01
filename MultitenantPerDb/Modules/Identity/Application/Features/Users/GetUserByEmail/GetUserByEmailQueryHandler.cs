using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Users.GetUserByEmail;

/// <summary>
/// Handler for GetUserByEmailQuery
/// </summary>
public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserByEmailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IUserRepository>();
        var user = await repository.GetByEmailAsync(request.Email);

        if (user == null)
            return null;

        return _mapper.Map<UserDto>(user);
    }
}
