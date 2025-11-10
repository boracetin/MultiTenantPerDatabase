namespace MultitenantPerDb.Modules.Identity.Domain.Constants;

/// <summary>
/// Constants specific to Identity module
/// </summary>
public static class IdentityConstants
{
    public static class ModuleConstants
    {
        public const string ModuleName = "Identity";
    }

    public static class ClaimTypes
    {
        public const string TenantId = "TenantId";
        public const string UserId = "UserId";
        public const string Email = "Email";
        public const string Role = "Role";
    }

    public static class PasswordRequirements
    {
        public const int MinLength = 6;
        public const int MaxLength = 100;
        public const bool RequireDigit = true;
        public const bool RequireUppercase = true;
        public const bool RequireLowercase = true;
        public const bool RequireNonAlphanumeric = false;
    }

    public static class ErrorMessages
    {
        public const string InvalidCredentials = "Invalid username or password";
        public const string UserNotFound = "User not found";
        public const string UserInactive = "User account is inactive";
        public const string EmailAlreadyExists = "Email already exists";
        public const string UsernameAlreadyExists = "Username already exists";
        public const string WeakPassword = "Password does not meet requirements";
    }

    public static class Validation
    {
        public const int UsernameMaxLength = 50;
        public const int UsernameMinLength = 3;
        public const int EmailMaxLength = 100;
    }

    public static class TokenSettings
    {
        public const int DefaultExpirationHours = 24;
        public const int RefreshTokenExpirationDays = 7;
    }
}
