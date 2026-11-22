#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application user settings related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Error types for application user settings operations.
    /// </summary>
    /// <remarks>
    /// The playback and VU meter bounds in this catalog are deliberately application-level validation: they constrain the shape and presentation range of the
    /// settings a caller submits, not business invariants of the user settings aggregate. A rule that becomes a business invariant must move to the aggregate
    /// and its error back to <c>Domain/Common/Errors</c>.
    /// </remarks>
    public static class UserSettings
    {
        /// <summary>
        /// Gets the error that reports that the number of items shown per page must be greater than zero.
        /// </summary>
        public static Error ItemsPerPageMustBeGreaterThanZero => Error.Validation(description: nameof(ItemsPerPageMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that the playback volume cannot be <see langword="null"/>.
        /// </summary>
        public static Error VolumeCannotBeNull => Error.Validation(description: nameof(VolumeCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the playback volume must be between 0 and 1, inclusive.
        /// </summary>
        public static Error VolumeMustBeBetweenZeroAndOne => Error.Validation(description: nameof(VolumeMustBeBetweenZeroAndOne));

        /// <summary>
        /// Gets the error that reports that the mute state cannot be <see langword="null"/>.
        /// </summary>
        public static Error IsMutedCannotBeNull => Error.Validation(description: nameof(IsMutedCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the option to keep downloading while playback is paused cannot be <see langword="null"/>.
        /// </summary>
        public static Error ShouldKeepDownloadingWhilePausedCannotBeNull => Error.Validation(description: nameof(ShouldKeepDownloadingWhilePausedCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the autoplay option cannot be <see langword="null"/>.
        /// </summary>
        public static Error ShouldAutoplayCannotBeNull => Error.Validation(description: nameof(ShouldAutoplayCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the gapless playback option cannot be <see langword="null"/>.
        /// </summary>
        public static Error IsGaplessPlaybackEnabledCannotBeNull => Error.Validation(description: nameof(IsGaplessPlaybackEnabledCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the replay gain mode cannot be <see langword="null"/>.
        /// </summary>
        public static Error ReplayGainModeCannotBeNull => Error.Validation(description: nameof(ReplayGainModeCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the streaming quality cannot be <see langword="null"/>.
        /// </summary>
        public static Error StreamingQualityCannotBeNull => Error.Validation(description: nameof(StreamingQualityCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the replay gain mode is not a recognized value.
        /// </summary>
        public static Error InvalidReplayGainMode => Error.Validation(description: nameof(InvalidReplayGainMode));

        /// <summary>
        /// Gets the error that reports that the streaming quality is not a recognized value.
        /// </summary>
        public static Error InvalidStreamingQuality => Error.Validation(description: nameof(InvalidStreamingQuality));

        /// <summary>
        /// Gets the error that reports that the VU meter orientation cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterOrientationCannotBeNull => Error.Validation(description: nameof(VuMeterOrientationCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the VU meter orientation is not a recognized value.
        /// </summary>
        public static Error InvalidVuMeterOrientation => Error.Validation(description: nameof(InvalidVuMeterOrientation));

        /// <summary>
        /// Gets the error that reports that the VU meter segment count cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterSegmentCountCannotBeNull => Error.Validation(description: nameof(VuMeterSegmentCountCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the VU meter segment count must be between 1 and 128, inclusive.
        /// </summary>
        public static Error VuMeterSegmentCountMustBeBetweenOneAndOneHundredTwentyEight => Error.Validation(description: nameof(VuMeterSegmentCountMustBeBetweenOneAndOneHundredTwentyEight));

        /// <summary>
        /// Gets the error that reports that the VU meter segment gap cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterSegmentGapCannotBeNull => Error.Validation(description: nameof(VuMeterSegmentGapCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the gap between the VU meter segments, in pixels, must be between 0 and 32, inclusive.
        /// </summary>
        public static Error VuMeterSegmentGapMustBeBetweenZeroAndThirtyTwo => Error.Validation(description: nameof(VuMeterSegmentGapMustBeBetweenZeroAndThirtyTwo));

        /// <summary>
        /// Gets the error that reports that the VU meter medium threshold cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterMediumThresholdCannotBeNull => Error.Validation(description: nameof(VuMeterMediumThresholdCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the VU meter medium threshold must be between 0 and 1, inclusive.
        /// </summary>
        public static Error VuMeterMediumThresholdMustBeBetweenZeroAndOne => Error.Validation(description: nameof(VuMeterMediumThresholdMustBeBetweenZeroAndOne));

        /// <summary>
        /// Gets the error that reports that the VU meter high threshold cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterHighThresholdCannotBeNull => Error.Validation(description: nameof(VuMeterHighThresholdCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the VU meter high threshold must be between 0 and 1, inclusive.
        /// </summary>
        public static Error VuMeterHighThresholdMustBeBetweenZeroAndOne => Error.Validation(description: nameof(VuMeterHighThresholdMustBeBetweenZeroAndOne));

        /// <summary>
        /// Gets the error that reports that the VU meter high threshold is lower than its medium threshold.
        /// </summary>
        public static Error VuMeterHighThresholdMustBeGreaterThanOrEqualToMediumThreshold => Error.Validation(description: nameof(VuMeterHighThresholdMustBeGreaterThanOrEqualToMediumThreshold));

        /// <summary>
        /// Gets the error that reports that the option to show the VU meter peak cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterShowPeakCannotBeNull => Error.Validation(description: nameof(VuMeterShowPeakCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the VU meter peak hold duration cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterPeakHoldDurationCannotBeNull => Error.Validation(description: nameof(VuMeterPeakHoldDurationCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the VU meter peak hold duration, in milliseconds, must be between 0 and 10000, inclusive.
        /// </summary>
        public static Error VuMeterPeakHoldDurationMustBeBetweenZeroAndTenThousandMilliseconds => Error.Validation(description: nameof(VuMeterPeakHoldDurationMustBeBetweenZeroAndTenThousandMilliseconds));

        /// <summary>
        /// Gets the error that reports that the VU meter release rate per second cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterReleasePerSecondCannotBeNull => Error.Validation(description: nameof(VuMeterReleasePerSecondCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the VU meter release rate per second must be between 0 and 100, inclusive.
        /// </summary>
        public static Error VuMeterReleasePerSecondMustBeBetweenZeroAndOneHundred => Error.Validation(description: nameof(VuMeterReleasePerSecondMustBeBetweenZeroAndOneHundred));

        /// <summary>
        /// Gets the error that reports that the VU meter peak release rate per second cannot be <see langword="null"/>.
        /// </summary>
        public static Error VuMeterPeakReleasePerSecondCannotBeNull => Error.Validation(description: nameof(VuMeterPeakReleasePerSecondCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the VU meter peak release rate per second must be between 0 and 100, inclusive.
        /// </summary>
        public static Error VuMeterPeakReleasePerSecondMustBeBetweenZeroAndOneHundred => Error.Validation(description: nameof(VuMeterPeakReleasePerSecondMustBeBetweenZeroAndOneHundred));
    }
}
