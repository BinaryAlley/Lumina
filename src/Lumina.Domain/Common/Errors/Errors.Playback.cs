#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Domain playback related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report playback delivery and track normalization failures.
    /// </summary>
    public static class Playback
    {
        /// <summary>
        /// Gets the error that reports that no supported playback format is available for the requested track.
        /// </summary>
        public static Error NoSupportedPlaybackFormat => Error.Forbidden(description: nameof(NoSupportedPlaybackFormat));

        /// <summary>
        /// Gets the error that reports that the playback delivery changed before it could be used.
        /// </summary>
        public static Error PlaybackDeliveryChanged => Error.Conflict(description: nameof(PlaybackDeliveryChanged));

        /// <summary>
        /// Gets the error that reports that the requested playback delivery was not found.
        /// </summary>
        public static Error PlaybackDeliveryNotFound => Error.NotFound(description: nameof(PlaybackDeliveryNotFound));

        /// <summary>
        /// Gets the error that reports that the playback delivery identifier cannot be empty.
        /// </summary>
        public static Error DeliveryIdCannotBeEmpty => Error.Validation(description: nameof(DeliveryIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the playback delivery identifier is not valid.
        /// </summary>
        public static Error DeliveryIdIsInvalid => Error.Validation(description: nameof(DeliveryIdIsInvalid));

        /// <summary>
        /// Gets the error that reports that the playback format has no implementation.
        /// </summary>
        public static Error PlaybackFormatNotImplemented => Error.Internal(description: nameof(PlaybackFormatNotImplemented));

        /// <summary>
        /// Gets the error that reports that the normalized track cache entry was not found.
        /// </summary>
        public static Error NormalizedTrackCacheEntryNotFound => Error.NotFound(description: nameof(NormalizedTrackCacheEntryNotFound));

        /// <summary>
        /// Gets the error that reports that the normalized track segment was not found.
        /// </summary>
        public static Error NormalizedTrackSegmentNotFound => Error.NotFound(description: nameof(NormalizedTrackSegmentNotFound));

        /// <summary>
        /// Gets the error that reports that the track normalization failed.
        /// </summary>
        public static Error NormalizationFailed => Error.Failure(description: nameof(NormalizationFailed));

        /// <summary>
        /// Gets the error that reports that the normalized track cache limit was exceeded.
        /// </summary>
        public static Error NormalizationCacheLimitExceeded => Error.Failure(description: nameof(NormalizationCacheLimitExceeded));

        /// <summary>
        /// Gets the error that reports that the track normalization timed out.
        /// </summary>
        public static Error NormalizationTimedOut => Error.Timeout(description: nameof(NormalizationTimedOut));

        /// <summary>
        /// Gets the error that reports that the track normalization is unavailable because a required resource cannot be used.
        /// </summary>
        public static Error NormalizationUnavailable => Error.ResourceUnavailable(description: nameof(NormalizationUnavailable));

        /// <summary>
        /// Gets the error that reports that normalization is not required for a directly played track.
        /// </summary>
        public static Error NormalizationIsNotRequiredForDirectPlay => Error.Conflict(description: nameof(NormalizationIsNotRequiredForDirectPlay));

        /// <summary>
        /// Gets the error that reports that the segment duration must be a positive number.
        /// </summary>
        public static Error SegmentDurationMustBePositive => Error.Validation(description: nameof(SegmentDurationMustBePositive));

        /// <summary>
        /// Gets the error that reports that the segment index cannot be empty.
        /// </summary>
        public static Error SegmentIndexCannotBeEmpty => Error.Validation(description: nameof(SegmentIndexCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the segment index is not valid.
        /// </summary>
        public static Error SegmentIndexMustBeValid => Error.Validation(description: nameof(SegmentIndexMustBeValid));

        /// <summary>
        /// Gets the error that reports that the track duration must be a positive number.
        /// </summary>
        public static Error TrackDurationMustBePositive => Error.Validation(description: nameof(TrackDurationMustBePositive));
    }
}
