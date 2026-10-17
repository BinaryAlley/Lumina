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
        public static Error PlaybackFormatNotImplemented => Error.Internal(description: nameof(PlaybackFormatNotImplemented));
        public static Error NormalizedTrackCacheEntryNotFound => Error.NotFound(description: nameof(NormalizedTrackCacheEntryNotFound));
        public static Error NormalizationFailed => Error.Failure(description: nameof(NormalizationFailed));
        public static Error NormalizationCacheLimitExceeded => Error.Failure(description: nameof(NormalizationCacheLimitExceeded));
        public static Error NormalizationTimedOut => Error.Timeout(description: nameof(NormalizationTimedOut));
        public static Error NormalizationUnavailable => Error.ResourceUnavailable(description: nameof(NormalizationUnavailable));
        public static Error NormalizationIsNotRequiredForDirectPlay => Error.InvalidOperation(description: nameof(NormalizationIsNotRequiredForDirectPlay));
        public static Error SegmentDurationMustBePositive => Error.Validation(description: nameof(SegmentDurationMustBePositive));
        public static Error TrackDurationMustBePositive => Error.Validation(description: nameof(TrackDurationMustBePositive));
    }
}
