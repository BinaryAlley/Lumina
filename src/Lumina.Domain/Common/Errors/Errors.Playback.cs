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
        public static Error PlaybackDeliveryChanged => Error.Conflict(description: nameof(PlaybackDeliveryChanged));
        public static Error PlaybackDeliveryNotFound => Error.NotFound(description: nameof(PlaybackDeliveryNotFound));
        public static Error DeliveryIdCannotBeEmpty => Error.Validation(description: nameof(DeliveryIdCannotBeEmpty));
        public static Error DeliveryIdIsInvalid => Error.Validation(description: nameof(DeliveryIdIsInvalid));
        public static Error PlaybackFormatNotImplemented => Error.Internal(description: nameof(PlaybackFormatNotImplemented));
        public static Error NormalizedTrackCacheEntryNotFound => Error.NotFound(description: nameof(NormalizedTrackCacheEntryNotFound));
        public static Error NormalizedTrackSegmentNotFound => Error.NotFound(description: nameof(NormalizedTrackSegmentNotFound));
        public static Error NormalizationFailed => Error.Failure(description: nameof(NormalizationFailed));
        public static Error NormalizationCacheLimitExceeded => Error.Failure(description: nameof(NormalizationCacheLimitExceeded));
        public static Error NormalizationTimedOut => Error.Timeout(description: nameof(NormalizationTimedOut));
        public static Error NormalizationUnavailable => Error.ResourceUnavailable(description: nameof(NormalizationUnavailable));
        public static Error NormalizationIsNotRequiredForDirectPlay => Error.Conflict(description: nameof(NormalizationIsNotRequiredForDirectPlay));
        public static Error SegmentDurationMustBePositive => Error.Validation(description: nameof(SegmentDurationMustBePositive));
        public static Error SegmentIndexCannotBeEmpty => Error.Validation(description: nameof(SegmentIndexCannotBeEmpty));
        public static Error SegmentIndexMustBeValid => Error.Validation(description: nameof(SegmentIndexMustBeValid));
        public static Error TrackDurationMustBePositive => Error.Validation(description: nameof(TrackDurationMustBePositive));
    }
}
