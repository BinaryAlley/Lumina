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
    }
}
