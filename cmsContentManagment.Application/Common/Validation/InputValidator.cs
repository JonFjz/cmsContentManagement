using System.Text.RegularExpressions;
using cmsContentManagment.Application.Common.ErrorCodes;

namespace cmsUserManagment.Application.Common.Validation;

public static class InputValidator
{
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    private static readonly Regex PasswordRegex =
        new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$", RegexOptions.Compiled);


    public static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            throw GeneralErrorCodes.InvalidEmailFormat;
    }

    public static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || !PasswordRegex.IsMatch(password))
            throw GeneralErrorCodes.PasswordTooWeak;
    }

    public static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 5)
            throw GeneralErrorCodes.UsernameTooShort;
    }
}
