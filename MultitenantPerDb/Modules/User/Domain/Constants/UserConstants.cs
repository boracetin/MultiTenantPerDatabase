namespace MultitenantPerDb.Modules.User.Domain.Constants;

/// <summary>
/// Constants specific to User module
/// </summary>
public static class UserConstants
{
    public static class UserStatus
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
        public const string Pending = "Pending";
        public const string Suspended = "Suspended";
    }

    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found";
        public const string UserInactive = "User is not active";
        public const string InvalidUserId = "Invalid user ID";
    }

    public static class Validation
    {
        public const int FirstNameMaxLength = 50;
        public const int LastNameMaxLength = 50;
        public const int PhoneMaxLength = 20;
    }
}
