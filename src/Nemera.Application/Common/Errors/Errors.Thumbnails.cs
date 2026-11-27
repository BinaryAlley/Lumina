#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application thumbnails related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Error types for application thumbnail operations.
    /// </summary>
    public static class Thumbnails
    {
        /// <summary>
        /// Gets the error that reports that the image quality must be between 0 and 100.
        /// </summary>
        public static Error ImageQualityMustBeBetweenZeroAndOneHundred => Error.Validation(description: nameof(ImageQualityMustBeBetweenZeroAndOneHundred));
    }
}
