#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Infrastructure.Common.Errors;

/// <summary>
/// Media Library Scan error types.
/// </summary>
public static partial class Errors
{
    public static class LibraryScanning
    {
        public static Error FailedToCreateScanJobProgress => Error.Failure(description: nameof(FailedToCreateScanJobProgress));
    }
}
