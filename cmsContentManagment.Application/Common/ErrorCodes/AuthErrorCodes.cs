namespace cmsContentManagment.Application.Common.ErrorCodes;

public class AuthErrorCodes : Exception
{
    public AuthErrorCodes(int code, string message) : base(message)
    {
        Code = code;
    }

    public int Code { get; }

    public static AuthErrorCodes TokenNotFound
        => new(1, "Authentication token was not provided or cannot be found.");

    public static AuthErrorCodes InvalidVerificationCode
        => new(2, "The provided verification code is incorrect or expired.");

    public static AuthErrorCodes FailedToLogOut
        => new(3, "An error occurred while logging out the user.");

    public static AuthErrorCodes BadToken
        => new(4, "The authentication token is invalid, corrupted, or expired.");

    public static AuthErrorCodes Unauthorized
        => new(5, "User is not authorized to perform this action.");

    public static AuthErrorCodes Forbidden
        => new(6, "Access to the requested resource is forbidden.");

    public static AuthErrorCodes SessionExpired
        => new(7, "User session has expired. Please log in again.");



    public static AuthErrorCodes LoginFailed
        => new(8, "Login failed due to invalid username or password.");

    public static AuthErrorCodes TooManyAttempts
        => new(9, "Too many login attempts. Please try again later.");

    public static AuthErrorCodes AccountLocked
        => new(10, "The user account has been locked due to security policies.");

    public static AuthErrorCodes InvalidCredentials
        => new(11, "There has been an error validating your credentials.");
}
