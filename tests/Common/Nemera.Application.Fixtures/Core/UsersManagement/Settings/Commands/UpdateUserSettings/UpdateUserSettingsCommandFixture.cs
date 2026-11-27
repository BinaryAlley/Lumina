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
    /// <param name="includeVuMeterOrientation">Whether to generate the VU meter orientation, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterSegmentCount">Whether to generate the VU meter segment count, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterSegmentGap">Whether to generate the VU meter segment gap, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterMediumThreshold">Whether to generate the VU meter medium threshold, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterHighThreshold">Whether to generate the VU meter high threshold, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterShowPeak">Whether to generate the VU meter peak marker setting, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterPeakHoldDurationInMilliseconds">Whether to generate the VU meter peak hold duration, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterReleasePerSecond">Whether to generate the VU meter release rate, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterPeakReleasePerSecond">Whether to generate the VU meter peak release rate, or leave it <see langword="null"/>.</param>
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
        double? vuMeterPeakReleasePerSecond = null,
        bool includeVuMeterOrientation = true,
        bool includeVuMeterSegmentCount = true,
        bool includeVuMeterSegmentGap = true,
        bool includeVuMeterMediumThreshold = true,
        bool includeVuMeterHighThreshold = true,
        bool includeVuMeterShowPeak = true,
        bool includeVuMeterPeakHoldDurationInMilliseconds = true,
        bool includeVuMeterReleasePerSecond = true,
        bool includeVuMeterPeakReleasePerSecond = true)
    {
        // The high threshold is generated from the medium one, so a randomized command always describes a valid meter.
        double? mediumThreshold = includeVuMeterMediumThreshold
            ? vuMeterMediumThreshold ?? Random.Shared.NextDouble()
            : null;
        double? highThreshold = includeVuMeterHighThreshold
            ? vuMeterHighThreshold ?? (mediumThreshold is null ? Random.Shared.NextDouble() : mediumThreshold.Value + Random.Shared.NextDouble() * (1 - mediumThreshold.Value))
            : null;
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
            .RuleFor(x => x.VuMeterOrientation, f => includeVuMeterOrientation ? vuMeterOrientation ?? f.PickRandom<VuMeterOrientation>() : null)
            .RuleFor(x => x.VuMeterSegmentCount, f => includeVuMeterSegmentCount ? vuMeterSegmentCount ?? f.Random.Int(1, 128) : null)
            .RuleFor(x => x.VuMeterSegmentGap, f => includeVuMeterSegmentGap ? vuMeterSegmentGap ?? f.Random.Int(0, 32) : null)
            .RuleFor(x => x.VuMeterMediumThreshold, f => mediumThreshold)
            .RuleFor(x => x.VuMeterHighThreshold, f => highThreshold)
            .RuleFor(x => x.VuMeterShowPeak, f => includeVuMeterShowPeak ? vuMeterShowPeak ?? f.Random.Bool() : null)
            .RuleFor(x => x.VuMeterPeakHoldDurationInMilliseconds, f => includeVuMeterPeakHoldDurationInMilliseconds ? vuMeterPeakHoldDurationInMilliseconds ?? f.Random.Int(0, 10000) : null)
            .RuleFor(x => x.VuMeterReleasePerSecond, f => includeVuMeterReleasePerSecond ? vuMeterReleasePerSecond ?? f.Random.Double(0, 100) : null)
            .RuleFor(x => x.VuMeterPeakReleasePerSecond, f => includeVuMeterPeakReleasePerSecond ? vuMeterPeakReleasePerSecond ?? f.Random.Double(0, 100) : null)
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
