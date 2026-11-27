#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Infrastructure.Common.Errors;

/// <summary>
/// Media Library Scan error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Media library scan error types.
    /// </summary>
    public static class LibraryScanning
    {
        /// <summary>
        /// Gets the error that reports a failure when the progress tracker of a media library scan job could not be created.
        /// </summary>
        public static Error FailedToCreateScanJobProgress => Error.Failure(description: nameof(FailedToCreateScanJobProgress));
    }
}
