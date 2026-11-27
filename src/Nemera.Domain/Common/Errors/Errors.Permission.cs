#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// Domain permission related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report denied access to file system resources.
    /// </summary>
    public static class Permission
    {
        /// <summary>
        /// Gets the error that reports that the current user is not authorized to access the requested resource.
        /// </summary>
        public static Error UnauthorizedAccess => Error.Failure(description: nameof(UnauthorizedAccess));
    }
}
