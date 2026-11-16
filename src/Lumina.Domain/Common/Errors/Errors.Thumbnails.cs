#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

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

        /// <summary>
        /// Gets the error that reports that the image quality must be between 0 and 100.
        /// </summary>
        public static Error ImageQualityMustBeBetweenZeroAndOneHundred => Error.Validation(description: nameof(ImageQualityMustBeBetweenZeroAndOneHundred));
    }
}
