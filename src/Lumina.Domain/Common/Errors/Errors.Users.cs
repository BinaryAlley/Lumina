#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Users error types.
/// </summary>
public static partial class Errors
{
    public static class Users
    {
        public static Error UserAlreadyExists => Error.Conflict(description: nameof(UserAlreadyExists));
        public static Error UserDoesNotExist => Error.NotFound(description: nameof(UserDoesNotExist));
        public static Error UserIdCannotBeEmpty => Error.Validation(description: nameof(UserIdCannotBeEmpty));
    }
}
