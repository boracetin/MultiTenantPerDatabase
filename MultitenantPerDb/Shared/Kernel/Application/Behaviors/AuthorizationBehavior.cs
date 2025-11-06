using MediatR;
using MultitenantPerDb.Shared.Kernel.Application.Interfaces;
using MultitenantPerDb.Shared.Kernel.Domain.Exceptions;

namespace MultitenantPerDb.Shared.Kernel.Application.Behaviors;

/// <summary>
/// Pipeline behavior for authorization and tenant isolation
/// Checks if user has required permissions/roles and enforces tenant isolation
/// CRITICAL for multi-tenant security!
/// </summary>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    public AuthorizationBehavior(
        ICurrentUserService currentUserService,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Check if request requires authorization
        if (request is not IAuthorizedRequest authorizedRequest)
        {
            _logger.LogDebug("[AUTHORIZATION SKIPPED] {RequestName} - Not an IAuthorizedRequest", requestName);
            return await next();
        }

        _logger.LogDebug(
            "[AUTHORIZATION START] {RequestName} - User: {UserId} - Tenant: {TenantId}",
            requestName,
            _currentUserService.UserId ?? "Anonymous",
            _currentUserService.TenantId ?? "N/A");

        // Check if user is authenticated
        if (!_currentUserService.IsAuthenticated)
        {
            _logger.LogWarning(
                "[AUTHORIZATION FAILED] {RequestName} - User not authenticated",
                requestName);

            throw new UnauthorizedException(
                "You must be authenticated to perform this action.",
                "NotAuthenticated");
        }

        // Check tenant isolation
        if (authorizedRequest.RequireTenantIsolation)
        {
            if (string.IsNullOrWhiteSpace(_currentUserService.TenantId))
            {
                _logger.LogWarning(
                    "[AUTHORIZATION FAILED] {RequestName} - User: {UserId} - No tenant context",
                    requestName,
                    _currentUserService.UserId);

                throw new UnauthorizedException(
                    "Tenant context is required for this action.",
                    "MissingTenantContext");
            }

            _logger.LogDebug(
                "[TENANT ISOLATION] {RequestName} - Enforcing tenant: {TenantId}",
                requestName,
                _currentUserService.TenantId);
        }

        // Check required roles
        if (authorizedRequest.RequiredRoles?.Any() == true)
        {
            var hasRole = authorizedRequest.RequiredRoles.Any(role => _currentUserService.IsInRole(role));

            if (!hasRole)
            {
                _logger.LogWarning(
                    "[AUTHORIZATION FAILED] {RequestName} - User: {UserId} - Missing roles: {RequiredRoles} - User roles: {UserRoles}",
                    requestName,
                    _currentUserService.UserId,
                    string.Join(", ", authorizedRequest.RequiredRoles),
                    string.Join(", ", _currentUserService.Roles));

                throw new UnauthorizedException(
                    "You do not have the required role to perform this action.",
                    requestName,
                    string.Join(", ", authorizedRequest.RequiredRoles));
            }

            _logger.LogDebug(
                "[ROLE CHECK PASSED] {RequestName} - User has role: {Roles}",
                requestName,
                string.Join(", ", authorizedRequest.RequiredRoles));
        }

        // Check required permissions
        if (authorizedRequest.RequiredPermissions?.Any() == true)
        {
            var hasPermission = authorizedRequest.RequiredPermissions.Any(perm => _currentUserService.HasPermission(perm));

            if (!hasPermission)
            {
                _logger.LogWarning(
                    "[AUTHORIZATION FAILED] {RequestName} - User: {UserId} - Missing permissions: {RequiredPermissions} - User permissions: {UserPermissions}",
                    requestName,
                    _currentUserService.UserId,
                    string.Join(", ", authorizedRequest.RequiredPermissions),
                    string.Join(", ", _currentUserService.Permissions));

                throw new UnauthorizedException(
                    "You do not have the required permission to perform this action.",
                    requestName,
                    string.Join(", ", authorizedRequest.RequiredPermissions));
            }

            _logger.LogDebug(
                "[PERMISSION CHECK PASSED] {RequestName} - User has permission: {Permissions}",
                requestName,
                string.Join(", ", authorizedRequest.RequiredPermissions));
        }

        _logger.LogDebug(
            "[AUTHORIZATION SUCCESS] {RequestName} - User: {UserId} - Tenant: {TenantId}",
            requestName,
            _currentUserService.UserId,
            _currentUserService.TenantId);

        return await next();
    }
}
