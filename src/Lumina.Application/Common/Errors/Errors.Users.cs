#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application users related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Error types for application user operations.
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// Gets the error that reports that the user identifier cannot be empty.
        /// </summary>
        public static Error UserIdCannotBeEmpty => Error.Validation(description: nameof(UserIdCannotBeEmpty));
    }
}
