using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Identity.Application.Commands;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Identity.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Identity.Application.Handlers.Commands;

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
        var existingUser = await repository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"Username '{request.Username}' is already taken");
        }

        // Check if email already exists
        var existingEmail = await repository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new InvalidOperationException($"Email '{request.Email}' is already registered");
        }

        // Hash password (demo - production'da BCrypt kullanılmalı)
        var passwordHash = request.Password; // Demo için plain text

        // For simplicity, use TenantId = 1 (in real app, get from context or registration flow)
        var tenantId = 1;

        // Create user using factory method
        var user = User.Create(
            request.Username,
            request.Email,
            passwordHash,
            tenantId
        );

        // Add user to repository
        await repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
