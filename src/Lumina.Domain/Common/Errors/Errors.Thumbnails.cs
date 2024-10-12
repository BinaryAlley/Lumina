#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Domain thumbnails related error types.
/// </summary>
public static partial class Errors
{
    public static class Thumbnails
    {
        public static Error NoThumbnail => Error.Failure(nameof(NoThumbnail));
        public static Error ImageQaulityMustBeBetweenZeroAndOneHundred => Error.Validation(nameof(ImageQaulityMustBeBetweenZeroAndOneHundred));
    }
}
