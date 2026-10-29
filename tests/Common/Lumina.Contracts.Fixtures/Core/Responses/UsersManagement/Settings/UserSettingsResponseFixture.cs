#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.UsersManagement.Settings;

/// <summary>
/// Fixture class for the <see cref="UserSettingsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UserSettingsResponse"/>.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Optional. Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">Optional. The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the "The" prefix is ignored by the alpha picker.</param>
    /// <param name="isThemeCachingEnabled">Optional. Whether the theme data served to this user is cached.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Optional. Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Optional. Whether the artwork of the media library items is aggregated from multiple providers, when it is missing.</param>
    /// <param name="shouldRenderPdfAsImages">Optional. Whether PDF books are rendered as page images for the user.</param>
    /// <param name="shouldPreserveBookStyles">Optional. Whether the styles of the book content are preserved when it is rendered for the user.</param>
    /// <param name="shouldKeepDownloadingWhilePaused">Optional. Whether the player keeps downloading a track while playback is paused.</param>
    /// <param name="shouldAutoplay">Optional. Whether the player continues playback at the end of an album or artist.</param>
    /// <param name="isGaplessPlaybackEnabled">Optional. Whether the player transitions between consecutive tracks without a gap.</param>
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
    /// <returns>The created <see cref="UserSettingsResponse"/>.</returns>
    public UserSettingsResponse Create(
        Guid? userId = null,
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
        // The high threshold is generated from the medium one, so a randomized response always describes a valid meter.
        double mediumThreshold = vuMeterMediumThreshold ?? _faker.Random.Double(0, 1);
        double highThreshold = vuMeterHighThreshold ?? _faker.Random.Double(mediumThreshold, 1);
        return new UserSettingsResponse(
            userId ?? Guid.NewGuid(),
            isPaginationEnabled ?? _faker.Random.Bool(),
            itemsPerPage ?? _faker.Random.Int(1, 100),
            shouldIgnoreThePrefixForAlphaPicker ?? _faker.Random.Bool(),
            isThemeCachingEnabled ?? _faker.Random.Bool(),
            shouldAggregateMetadataWhenMissing ?? _faker.Random.Bool(),
            shouldAggregateArtworkWhenMissing ?? _faker.Random.Bool(),
            shouldRenderPdfAsImages ?? _faker.Random.Bool(),
            shouldPreserveBookStyles ?? _faker.Random.Bool(),
            shouldKeepDownloadingWhilePaused ?? _faker.Random.Bool(),
            shouldAutoplay ?? _faker.Random.Bool(),
            isGaplessPlaybackEnabled ?? _faker.Random.Bool(),
            replayGainMode ?? _faker.PickRandom<ReplayGainMode>(),
            streamingQuality ?? _faker.PickRandom<PlaybackStreamingQuality>(),
            vuMeterOrientation ?? _faker.PickRandom<VuMeterOrientation>(),
            vuMeterSegmentCount ?? _faker.Random.Int(1, 128),
            vuMeterSegmentGap ?? _faker.Random.Int(0, 32),
            mediumThreshold,
            highThreshold,
            vuMeterShowPeak ?? _faker.Random.Bool(),
            vuMeterPeakHoldDurationInMilliseconds ?? _faker.Random.Int(0, 10000),
            vuMeterReleasePerSecond ?? _faker.Random.Double(0, 100),
            vuMeterPeakReleasePerSecond ?? _faker.Random.Double(0, 100));
    }

    /// <summary>
    /// Creates a list of <see cref="UserSettingsResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UserSettingsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
