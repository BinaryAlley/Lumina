#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// User settings error types.
/// </summary>
public static partial class Errors
{
    public static class UserSettings
    {
        public static Error UserSettingsAlreadyExists => Error.Conflict(description: nameof(UserSettingsAlreadyExists));
        public static Error UserSettingsNotFound => Error.NotFound(description: nameof(UserSettingsNotFound));
        public static Error ItemsPerPageMustBeGreaterThanZero => Error.Validation(description: nameof(ItemsPerPageMustBeGreaterThanZero));
        public static Error VolumeCannotBeNull => Error.Validation(description: nameof(VolumeCannotBeNull));
        public static Error VolumeMustBeBetweenZeroAndOne => Error.Validation(description: nameof(VolumeMustBeBetweenZeroAndOne));
        public static Error IsMutedCannotBeNull => Error.Validation(description: nameof(IsMutedCannotBeNull));
        public static Error ShouldKeepDownloadingWhilePausedCannotBeNull => Error.Validation(description: nameof(ShouldKeepDownloadingWhilePausedCannotBeNull));
        public static Error ShouldAutoplayCannotBeNull => Error.Validation(description: nameof(ShouldAutoplayCannotBeNull));
        public static Error IsGaplessPlaybackEnabledCannotBeNull => Error.Validation(description: nameof(IsGaplessPlaybackEnabledCannotBeNull));
        public static Error ReplayGainModeCannotBeNull => Error.Validation(description: nameof(ReplayGainModeCannotBeNull));
        public static Error StreamingQualityCannotBeNull => Error.Validation(description: nameof(StreamingQualityCannotBeNull));
        public static Error InvalidReplayGainMode => Error.Validation(description: nameof(InvalidReplayGainMode));
        public static Error InvalidStreamingQuality => Error.Validation(description: nameof(InvalidStreamingQuality));
        public static Error VuMeterOrientationCannotBeNull => Error.Validation(description: nameof(VuMeterOrientationCannotBeNull));
        public static Error InvalidVuMeterOrientation => Error.Validation(description: nameof(InvalidVuMeterOrientation));
        public static Error VuMeterSegmentCountCannotBeNull => Error.Validation(description: nameof(VuMeterSegmentCountCannotBeNull));
        public static Error VuMeterSegmentCountMustBeBetweenOneAndOneHundredTwentyEight => Error.Validation(description: nameof(VuMeterSegmentCountMustBeBetweenOneAndOneHundredTwentyEight));
        public static Error VuMeterSegmentGapCannotBeNull => Error.Validation(description: nameof(VuMeterSegmentGapCannotBeNull));
        public static Error VuMeterSegmentGapMustBeBetweenZeroAndThirtyTwo => Error.Validation(description: nameof(VuMeterSegmentGapMustBeBetweenZeroAndThirtyTwo));
        public static Error VuMeterMediumThresholdCannotBeNull => Error.Validation(description: nameof(VuMeterMediumThresholdCannotBeNull));
        public static Error VuMeterMediumThresholdMustBeBetweenZeroAndOne => Error.Validation(description: nameof(VuMeterMediumThresholdMustBeBetweenZeroAndOne));
        public static Error VuMeterHighThresholdCannotBeNull => Error.Validation(description: nameof(VuMeterHighThresholdCannotBeNull));
        public static Error VuMeterHighThresholdMustBeBetweenZeroAndOne => Error.Validation(description: nameof(VuMeterHighThresholdMustBeBetweenZeroAndOne));
        public static Error VuMeterHighThresholdMustBeGreaterThanOrEqualToMediumThreshold => Error.Validation(description: nameof(VuMeterHighThresholdMustBeGreaterThanOrEqualToMediumThreshold));
        public static Error VuMeterShowPeakCannotBeNull => Error.Validation(description: nameof(VuMeterShowPeakCannotBeNull));
        public static Error VuMeterPeakHoldDurationCannotBeNull => Error.Validation(description: nameof(VuMeterPeakHoldDurationCannotBeNull));
        public static Error VuMeterPeakHoldDurationMustBeBetweenZeroAndTenThousandMilliseconds => Error.Validation(description: nameof(VuMeterPeakHoldDurationMustBeBetweenZeroAndTenThousandMilliseconds));
        public static Error VuMeterReleasePerSecondCannotBeNull => Error.Validation(description: nameof(VuMeterReleasePerSecondCannotBeNull));
        public static Error VuMeterReleasePerSecondMustBeBetweenZeroAndOneHundred => Error.Validation(description: nameof(VuMeterReleasePerSecondMustBeBetweenZeroAndOneHundred));
        public static Error VuMeterPeakReleasePerSecondCannotBeNull => Error.Validation(description: nameof(VuMeterPeakReleasePerSecondCannotBeNull));
        public static Error VuMeterPeakReleasePerSecondMustBeBetweenZeroAndOneHundred => Error.Validation(description: nameof(VuMeterPeakReleasePerSecondMustBeBetweenZeroAndOneHundred));
    }
}
