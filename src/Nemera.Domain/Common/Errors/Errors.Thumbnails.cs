#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// Domain thumbnails related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report thumbnail generation failures and image quality rule violations.
    /// </summary>
    public static class Thumbnails
    {
        /// <summary>
        /// Gets the error that reports that no thumbnail is available for the requested media item.
        /// </summary>
        public static Error NoThumbnail => Error.Failure(description: nameof(NoThumbnail));
    }
}
