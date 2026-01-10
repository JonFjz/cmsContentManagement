namespace cmsContentManagement.Application.Common.ErrorCodes;

public class GeneralErrorCodes : Exception
{
    public GeneralErrorCodes(int code, string message) : base(message)
    {
        Code = code;
    }

    public int Code { get; }

    public static GeneralErrorCodes NotFound
        => new(1, "The requested resource was not found.");

    public static GeneralErrorCodes UserAlreadyExists
        => new(2, "A user with the provided information already exists.");

    public static GeneralErrorCodes InvalidInput
        => new(3, "The provided input is invalid or missing required fields.");

    public static GeneralErrorCodes OperationFailed
        => new(4, "The requested operation failed due to an internal error.");

    public static GeneralErrorCodes DatabaseError
        => new(5, "A database-related error occurred.");

    public static GeneralErrorCodes Conflict
        => new(6, "A conflict occurred with an existing resource.");

    public static GeneralErrorCodes ServiceUnavailable
        => new(7, "The service is currently unavailable. Please try again later.");

    public static GeneralErrorCodes PermissionDenied
        => new(8, "You do not have permission to perform this operation.");

    public static GeneralErrorCodes ValidationError
        => new(9, "One or more validation errors occurred.");

    public static GeneralErrorCodes Unknown
        => new(10, "An unknown error has occurred.");

    public static GeneralErrorCodes PasswordTooWeak
        => new(11, "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.");

    public static GeneralErrorCodes UsernameTooShort
        => new(12, "Username must be at least 5 characters long.");

    public static GeneralErrorCodes InvalidEmailFormat
        => new(13, "Email format is invalid.");

    public static GeneralErrorCodes ContentAlreadyExists
        => new(14, "Content already exists. Use PUT to update.");

    public static GeneralErrorCodes ContentIsNew
        => new(15, "Content is new. Use POST to create.");

    public static GeneralErrorCodes TagNotFound(string tagName)
        => new(16, $"Tag '{tagName}' does not exist.");

    public static GeneralErrorCodes InvalidApiKey
        => new(17, "Invalid API Key");
}
