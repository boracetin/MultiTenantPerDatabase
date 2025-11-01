using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Identity.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Identity.Application.Features.Auth.Register;

/// <summary>
/// Handler for RegisterCommand
/// Creates a new user account
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IUserRepository>();

        // Check if username already exists
        var existingUserByUsername = await repository.GetByUsernameAsync(request.Username);
        if (existingUserByUsername != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email already exists
        var existingUserByEmail = await repository.GetByEmailAsync(request.Email);
        if (existingUserByEmail != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Hash password (demo - production'da BCrypt kullanılmalı)
        var passwordHash = request.Password; // Plain text for demo

        // Create user (demo için TenantId = 1 olarak hardcoded)
        var user = User.Create(
            username: request.Username,
            email: request.Email,
            passwordHash: passwordHash,
            tenantId: 1
        );

        await repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
