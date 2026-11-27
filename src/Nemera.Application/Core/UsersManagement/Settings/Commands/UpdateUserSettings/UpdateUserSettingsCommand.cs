#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Command for updating the settings of the current user.
/// </summary>
/// <param name="IsPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
/// <param name="ItemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
/// <param name="ShouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
/// <param name="IsThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
/// <param name="ShouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.</param>
/// <param name="ShouldAggregateArtworkWhenMissing">Whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not.</param>
/// <param name="ShouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user, or not.</param>
/// <param name="ShouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user, or not.</param>
/// <param name="ShouldKeepDownloadingWhilePaused">Whether the player keeps downloading a track while playback is paused, or not.</param>
/// <param name="ShouldAutoplay">Whether the player continues playback at the end of an album or artist, or not.</param>
/// <param name="IsGaplessPlaybackEnabled">Whether the player transitions between consecutive tracks without a gap, or not.</param>
/// <param name="ReplayGainMode">The way ReplayGain normalization is applied to the tracks the user plays.</param>
/// <param name="StreamingQuality">The quality cap applied to the audio the server normalizes for the user.</param>
/// <param name="VuMeterOrientation">The direction in which each channel of the VU meter of the audio player grows.</param>
/// <param name="VuMeterSegmentCount">The number of segments each channel of the VU meter is divided into.</param>
/// <param name="VuMeterSegmentGap">The gap, in pixels, between the segments and the channels of the VU meter.</param>
/// <param name="VuMeterMediumThreshold">The level, between 0 and 1, above which a segment of the VU meter is drawn with its medium state.</param>
/// <param name="VuMeterHighThreshold">The level, between 0 and 1, above which a segment of the VU meter is drawn with its high state.</param>
/// <param name="VuMeterShowPeak">Whether the VU meter holds the peak level of each channel after it falls, or not.</param>
/// <param name="VuMeterPeakHoldDurationInMilliseconds">The time, in milliseconds, a peak of the VU meter is held before it starts falling.</param>
/// <param name="VuMeterReleasePerSecond">The number of level units the display of the VU meter falls per second when the input level drops.</param>
/// <param name="VuMeterPeakReleasePerSecond">The number of level units the peak of the VU meter falls per second after its hold time elapsed.</param>
public record UpdateUserSettingsCommand(
    bool IsPaginationEnabled,
    int ItemsPerPage,
    bool ShouldIgnoreThePrefixForAlphaPicker,
    bool IsThemeCachingEnabled,
    bool ShouldAggregateMetadataWhenMissing,
    bool ShouldAggregateArtworkWhenMissing,
    bool ShouldRenderPdfAsImages,
    bool ShouldPreserveBookStyles,
    bool? ShouldKeepDownloadingWhilePaused,
    bool? ShouldAutoplay,
    bool? IsGaplessPlaybackEnabled,
    ReplayGainMode? ReplayGainMode,
    PlaybackStreamingQuality? StreamingQuality,
    VuMeterOrientation? VuMeterOrientation,
    int? VuMeterSegmentCount,
    int? VuMeterSegmentGap,
    double? VuMeterMediumThreshold,
    double? VuMeterHighThreshold,
    bool? VuMeterShowPeak,
    int? VuMeterPeakHoldDurationInMilliseconds,
    double? VuMeterReleasePerSecond,
    double? VuMeterPeakReleasePerSecond
) : ICommand;
