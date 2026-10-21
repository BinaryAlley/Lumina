#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Fixture class for the <see cref="UpdateUserSettingsCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsCommandFixture
{
    /// <summary>
    /// Creates a random valid command to update the settings of the current user.
    /// </summary>
    /// <param name="isPaginationEnabled">Optional. Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">Optional. The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the "The" prefix is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Optional. Whether the theme data served to this user is cached, or not.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Optional. Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Optional. Whether the artwork of the media library items is aggregated from multiple providers, when it is missing.</param>
    /// <param name="shouldRenderPdfAsImages">Optional. Whether PDF books are rendered as page images for the user.</param>
    /// <param name="shouldPreserveBookStyles">Optional. Whether the styles of the book content are preserved when it is rendered for the user.</param>
    /// <param name="shouldKeepDownloadingWhilePaused">Optional. Whether the player keeps downloading a track while playback is paused, or not.</param>
    /// <param name="shouldAutoplay">Optional. Whether the player continues playback at the end of an album or artist, or not.</param>
    /// <param name="isGaplessPlaybackEnabled">Optional. Whether the player transitions between consecutive tracks without a gap, or not.</param>
    /// <param name="replayGainMode">Optional. The way ReplayGain normalization is applied to the tracks the user plays.</param>
    /// <param name="streamingQuality">Optional. The quality cap applied to the audio the server normalizes for the user.</param>
    /// <param name="vuMeterOrientation">Optional. The direction in which each channel of the VU meter of the audio player grows.</param>
    /// <param name="vuMeterSegmentCount">Optional. The number of segments each channel of the VU meter is divided into.</param>
    /// <param name="vuMeterSegmentGap">Optional. The gap, in pixels, between the segments and the channels of the VU meter.</param>
    /// <param name="vuMeterMediumThreshold">Optional. The level above which a segment of the VU meter is drawn with its medium state.</param>
    /// <param name="vuMeterHighThreshold">Optional. The level above which a segment of the VU meter is drawn with its high state.</param>
    /// <param name="vuMeterShowPeak">Optional. Whether the VU meter holds the peak level of each channel after it falls.</param>
    /// <param name="vuMeterPeakHoldDurationInMilliseconds">Optional. The time a peak of the VU meter is held before it starts falling.</param>
    /// <param name="vuMeterReleasePerSecond">Optional. The number of level units the display of the VU meter falls per second when the input level drops.</param>
    /// <param name="vuMeterPeakReleasePerSecond">Optional. The number of level units the peak of the VU meter falls per second after its hold time elapsed.</param>
    /// <returns>The created command.</returns>
    public UpdateUserSettingsCommand Create(
        bool? isPaginationEnabled = null,
        int? itemsPerPage = null,
        bool? shouldIgnoreThePrefixForAlphaPicker = null,
        bool? isThemeCachingEnabled = null,
        bool? shouldAggregateMetadataWhenMissing = null,
        bool? shouldAggregateArtworkWhenMissing = null,
        bool? shouldRenderPdfAsImages = null,
        bool? shouldPreserveBookStyles = null,
        bool? shouldKeepDownloadingWhilePaused = null,
        bool? shouldAutoplay = null,
        bool? isGaplessPlaybackEnabled = null,
        ReplayGainMode? replayGainMode = null,
        PlaybackStreamingQuality? streamingQuality = null,
        VuMeterOrientation? vuMeterOrientation = null,
        int? vuMeterSegmentCount = null,
        int? vuMeterSegmentGap = null,
        double? vuMeterMediumThreshold = null,
        double? vuMeterHighThreshold = null,
        bool? vuMeterShowPeak = null,
        int? vuMeterPeakHoldDurationInMilliseconds = null,
        double? vuMeterReleasePerSecond = null,
        double? vuMeterPeakReleasePerSecond = null)
    {
        // The high threshold is generated from the medium one, so a randomized command always describes a valid meter.
        double mediumThreshold = vuMeterMediumThreshold ?? Random.Shared.NextDouble();
        double highThreshold = vuMeterHighThreshold ?? (mediumThreshold + Random.Shared.NextDouble() * (1 - mediumThreshold));
        return new Faker<UpdateUserSettingsCommand>()
            .CustomInstantiator(f => new UpdateUserSettingsCommand(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default))
            .RuleFor(x => x.IsPaginationEnabled, f => isPaginationEnabled ?? f.Random.Bool())
            .RuleFor(x => x.ItemsPerPage, f => itemsPerPage ?? f.Random.Int(1, 100))
            .RuleFor(x => x.ShouldIgnoreThePrefixForAlphaPicker, f => shouldIgnoreThePrefixForAlphaPicker ?? f.Random.Bool())
            .RuleFor(x => x.IsThemeCachingEnabled, f => isThemeCachingEnabled ?? f.Random.Bool())
            .RuleFor(x => x.ShouldAggregateMetadataWhenMissing, f => shouldAggregateMetadataWhenMissing ?? f.Random.Bool())
            .RuleFor(x => x.ShouldAggregateArtworkWhenMissing, f => shouldAggregateArtworkWhenMissing ?? f.Random.Bool())
            .RuleFor(x => x.ShouldRenderPdfAsImages, f => shouldRenderPdfAsImages ?? f.Random.Bool())
            .RuleFor(x => x.ShouldPreserveBookStyles, f => shouldPreserveBookStyles ?? f.Random.Bool())
            .RuleFor(x => x.ShouldKeepDownloadingWhilePaused, f => shouldKeepDownloadingWhilePaused ?? f.Random.Bool())
            .RuleFor(x => x.ShouldAutoplay, f => shouldAutoplay ?? f.Random.Bool())
            .RuleFor(x => x.IsGaplessPlaybackEnabled, f => isGaplessPlaybackEnabled ?? f.Random.Bool())
            .RuleFor(x => x.ReplayGainMode, f => replayGainMode ?? f.PickRandom<ReplayGainMode>())
            .RuleFor(x => x.StreamingQuality, f => streamingQuality ?? f.PickRandom<PlaybackStreamingQuality>())
            .RuleFor(x => x.VuMeterOrientation, f => vuMeterOrientation ?? f.PickRandom<VuMeterOrientation>())
            .RuleFor(x => x.VuMeterSegmentCount, f => vuMeterSegmentCount ?? f.Random.Int(1, 128))
            .RuleFor(x => x.VuMeterSegmentGap, f => vuMeterSegmentGap ?? f.Random.Int(0, 32))
            .RuleFor(x => x.VuMeterMediumThreshold, f => mediumThreshold)
            .RuleFor(x => x.VuMeterHighThreshold, f => highThreshold)
            .RuleFor(x => x.VuMeterShowPeak, f => vuMeterShowPeak ?? f.Random.Bool())
            .RuleFor(x => x.VuMeterPeakHoldDurationInMilliseconds, f => vuMeterPeakHoldDurationInMilliseconds ?? f.Random.Int(0, 10000))
            .RuleFor(x => x.VuMeterReleasePerSecond, f => vuMeterReleasePerSecond ?? f.Random.Double(0, 100))
            .RuleFor(x => x.VuMeterPeakReleasePerSecond, f => vuMeterPeakReleasePerSecond ?? f.Random.Double(0, 100))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateUserSettingsCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateUserSettingsCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
