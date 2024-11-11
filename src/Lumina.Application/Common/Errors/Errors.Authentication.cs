#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application authentication related error types.
/// </summary>
public static partial class Errors
{
    public static class Authentication
    {
        public static Error UsernameAlreadyExists => Error.Conflict(description: nameof(UsernameAlreadyExists));
        public static Error UsernameCannotBeEmpty => Error.Validation(description: nameof(UsernameCannotBeEmpty));
        public static Error PasswordCannotBeEmpty => Error.Validation(description: nameof(PasswordCannotBeEmpty));
        public static Error PasswordConfirmCannotBeEmpty => Error.Validation(description: nameof(PasswordConfirmCannotBeEmpty));
        public static Error InvalidPassword => Error.Validation(description: nameof(InvalidPassword));
        public static Error PasswordsNotMatch => Error.Validation(description: nameof(PasswordsNotMatch));
        public static Error InvalidTotpCode => Error.Validation(description: nameof(InvalidTotpCode));
        public static Error InvalidUsernameOrPassword => Error.Failure(description: nameof(InvalidUsernameOrPassword));
    }
}
