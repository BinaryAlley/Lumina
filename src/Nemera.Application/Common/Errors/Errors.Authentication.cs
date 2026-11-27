#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Application.Common.Errors;

/// <summary>
/// Application authentication related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Error types for application authentication operations.
    /// </summary>
    public static class Authentication
    {
        /// <summary>
        /// Gets the error that reports that a username already exists.
        /// </summary>
        public static Error UsernameAlreadyExists => Error.Conflict(description: nameof(UsernameAlreadyExists));

        /// <summary>
        /// Gets the error that reports that a username does not exist.
        /// </summary>
        public static Error UsernameDoesNotExist => Error.NotFound(description: nameof(UsernameDoesNotExist));

        /// <summary>
        /// Gets the error that reports that a username cannot be empty.
        /// </summary>
        public static Error UsernameCannotBeEmpty => Error.Validation(description: nameof(UsernameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a time-based one-time password cannot be empty.
        /// </summary>
        public static Error TotpCannotBeEmpty => Error.Validation(description: nameof(TotpCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a password cannot be empty.
        /// </summary>
        public static Error PasswordCannotBeEmpty => Error.Validation(description: nameof(PasswordCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a new password cannot be empty.
        /// </summary>
        public static Error NewPasswordCannotBeEmpty => Error.Validation(description: nameof(NewPasswordCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a current password cannot be empty.
        /// </summary>
        public static Error CurrentPasswordCannotBeEmpty => Error.Validation(description: nameof(CurrentPasswordCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a password confirmation cannot be empty.
        /// </summary>
        public static Error PasswordConfirmCannotBeEmpty => Error.Validation(description: nameof(PasswordConfirmCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a new password confirmation cannot be empty.
        /// </summary>
        public static Error NewPasswordConfirmCannotBeEmpty => Error.Validation(description: nameof(NewPasswordConfirmCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a password is invalid.
        /// </summary>
        public static Error InvalidPassword => Error.Validation(description: nameof(InvalidPassword));

        /// <summary>
        /// Gets the error that reports that a username must be between 3 and 255 characters long.
        /// </summary>
        public static Error UsernameMustBeBetween3And255CharactersLong => Error.Validation(description: nameof(UsernameMustBeBetween3And255CharactersLong));

        /// <summary>
        /// Gets the error that reports that a username is invalid.
        /// </summary>
        public static Error InvalidUsername => Error.Validation(description: nameof(InvalidUsername));

        /// <summary>
        /// Gets the error that reports that a password and its confirmation do not match.
        /// </summary>
        public static Error PasswordsNotMatch => Error.Validation(description: nameof(PasswordsNotMatch));

        /// <summary>
        /// Gets the error that reports that a time-based one-time password code is invalid.
        /// </summary>
        public static Error InvalidTotpCode => Error.Validation(description: nameof(InvalidTotpCode));

        /// <summary>
        /// Gets the error that reports that the combination of a username and a password is invalid.
        /// </summary>
        public static Error InvalidUsernameOrPassword => Error.Failure(description: nameof(InvalidUsernameOrPassword));

        /// <summary>
        /// Gets the error that reports that a current password is invalid.
        /// </summary>
        public static Error InvalidCurrentPassword => Error.Failure(description: nameof(InvalidCurrentPassword));

        /// <summary>
        /// Gets the error that reports that a temporary password has expired.
        /// </summary>
        public static Error TempPasswordExpired => Error.Failure(description: nameof(TempPasswordExpired));

        /// <summary>
        /// Gets the error that reports that a password reset has already been requested.
        /// </summary>
        public static Error PasswordResetAlreadyRequested => Error.Failure(description: nameof(PasswordResetAlreadyRequested));
    }
}
