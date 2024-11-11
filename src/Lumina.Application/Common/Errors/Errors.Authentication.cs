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
        public static Error UsernameDoesNotExist => Error.NotFound(description: nameof(UsernameDoesNotExist));
        public static Error UsernameCannotBeEmpty => Error.Validation(description: nameof(UsernameCannotBeEmpty));
        public static Error TotpCannotBeEmpty => Error.Validation(description: nameof(TotpCannotBeEmpty));
        public static Error PasswordCannotBeEmpty => Error.Validation(description: nameof(PasswordCannotBeEmpty));
        public static Error NewPasswordCannotBeEmpty => Error.Validation(description: nameof(NewPasswordCannotBeEmpty));
        public static Error CurrentPasswordCannotBeEmpty => Error.Validation(description: nameof(CurrentPasswordCannotBeEmpty));
        public static Error PasswordConfirmCannotBeEmpty => Error.Validation(description: nameof(PasswordConfirmCannotBeEmpty));
        public static Error NewPasswordConfirmCannotBeEmpty => Error.Validation(description: nameof(NewPasswordConfirmCannotBeEmpty));
        public static Error InvalidPassword => Error.Validation(description: nameof(InvalidPassword));
        public static Error PasswordsNotMatch => Error.Validation(description: nameof(PasswordsNotMatch));
        public static Error InvalidTotpCode => Error.Validation(description: nameof(InvalidTotpCode));
        public static Error InvalidUsernameOrPassword => Error.Failure(description: nameof(InvalidUsernameOrPassword));
        public static Error InvalidCurrentPassword => Error.Failure(description: nameof(InvalidCurrentPassword));
        public static Error TempPasswordExpired => Error.Failure(description: nameof(TempPasswordExpired));
        public static Error PasswordResetAlreadyRequested => Error.Failure(description: nameof(PasswordResetAlreadyRequested));
    }
}
