#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Users error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report user rule violations and missing users.
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// Gets the error that reports that the user already exists.
        /// </summary>
        public static Error UserAlreadyExists => Error.Conflict(description: nameof(UserAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the requested user does not exist.
        /// </summary>
        public static Error UserDoesNotExist => Error.NotFound(description: nameof(UserDoesNotExist));
    }
}
