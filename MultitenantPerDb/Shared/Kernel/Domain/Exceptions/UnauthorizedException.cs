namespace MultitenantPerDb.Shared.Kernel.Domain.Exceptions;

/// <summary>
/// Exception thrown when authorization fails
/// </summary>
public class UnauthorizedException : Exception
{
    public string? Reason { get; }
    public string? ResourceType { get; }
    public string? RequiredPermissions { get; }

    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, string reason) : base(message)
    {
        Reason = reason;
    }

    public UnauthorizedException(
        string message, 
        string resourceType, 
        string requiredPermissions) : base(message)
    {
        ResourceType = resourceType;
        RequiredPermissions = requiredPermissions;
    }
}
