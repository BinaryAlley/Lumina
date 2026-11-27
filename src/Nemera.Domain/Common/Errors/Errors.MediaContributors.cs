#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// Media contributor error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report media item contributor name rule violations.
    /// </summary>
    public static class MediaContributors
    {
        /// <summary>
        /// Gets the error that reports that the name of a media contributor cannot be empty.
        /// </summary>
        public static Error MediaContributorNameCannotBeEmpty => Error.Validation(description: nameof(MediaContributorNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the role name of a media contributor cannot be empty.
        /// </summary>
        public static Error MediaContributorRoleNameCannotBeEmpty => Error.Validation(description: nameof(MediaContributorRoleNameCannotBeEmpty));
    }
}
