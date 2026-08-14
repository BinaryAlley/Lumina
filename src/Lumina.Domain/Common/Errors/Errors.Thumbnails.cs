#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Domain thumbnails related error types.
/// </summary>
public static partial class Errors
{
    public static class Thumbnails
    {
        public static Error NoThumbnail => Error.Failure(description: nameof(NoThumbnail));
        public static Error ImageQualityMustBeBetweenZeroAndOneHundred => Error.Validation(description: nameof(ImageQualityMustBeBetweenZeroAndOneHundred));
    }
}
