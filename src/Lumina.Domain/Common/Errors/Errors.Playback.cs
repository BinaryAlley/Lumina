#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Domain playback related error types.
/// </summary>
public static partial class Errors
{
    public static class Playback
    {
        public static Error NoSupportedPlaybackFormat => Error.Forbidden(description: nameof(NoSupportedPlaybackFormat));
    }
}
