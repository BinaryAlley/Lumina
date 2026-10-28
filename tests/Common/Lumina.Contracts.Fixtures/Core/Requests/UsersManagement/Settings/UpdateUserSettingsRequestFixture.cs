#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;

/// <summary>
/// Fixture class for the <see cref="UpdateUserSettingsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdateUserSettingsRequest"/>.
    /// </summary>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix is ignored by the alpha picker.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork of the media library items is aggregated from multiple providers, when it is missing.</param>
    /// <param name="shouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user.</param>
    /// <param name="shouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user.</param>
    /// <param name="shouldKeepDownloadingWhilePaused">Whether the player keeps downloading a track while playback is paused.</param>
    /// <param name="shouldAutoplay">Whether the player continues playback at the end of an album or artist.</param>
    /// <param name="isGaplessPlaybackEnabled">Whether the player transitions between consecutive tracks without a gap.</param>
    /// <param name="replayGainMode">The way ReplayGain normalization is applied to the tracks the user plays.</param>
    /// <param name="streamingQuality">The quality cap applied to the audio the server normalizes for the user.</param>
    /// <param name="vuMeterOrientation">The direction in which each channel of the VU meter of the audio player grows.</param>
    /// <param name="vuMeterSegmentCount">The number of segments each channel of the VU meter is divided into.</param>
    /// <param name="vuMeterSegmentGap">The gap, in pixels, between the segments and the channels of the VU meter.</param>
    /// <param name="vuMeterMediumThreshold">The level above which a segment of the VU meter is drawn with its medium state.</param>
    /// <param name="vuMeterHighThreshold">The level above which a segment of the VU meter is drawn with its high state.</param>
    /// <param name="vuMeterShowPeak">Whether the VU meter holds the peak level of each channel after it falls.</param>
    /// <param name="vuMeterPeakHoldDurationInMilliseconds">The time a peak of the VU meter is held before it starts falling.</param>
    /// <param name="vuMeterReleasePerSecond">The number of level units the display of the VU meter falls per second when the input level drops.</param>
    /// <param name="vuMeterPeakReleasePerSecond">The number of level units the peak of the VU meter falls per second after its hold time elapsed.</param>
    /// <param name="includeVuMeterOrientation">Whether to generate the VU meter orientation, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterSegmentCount">Whether to generate the VU meter segment count, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterSegmentGap">Whether to generate the VU meter segment gap, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterMediumThreshold">Whether to generate the VU meter medium threshold, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterHighThreshold">Whether to generate the VU meter high threshold, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterShowPeak">Whether to generate the VU meter peak marker setting, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterPeakHoldDurationInMilliseconds">Whether to generate the VU meter peak hold duration, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterReleasePerSecond">Whether to generate the VU meter release rate, or leave it <see langword="null"/>.</param>
    /// <param name="includeVuMeterPeakReleasePerSecond">Whether to generate the VU meter peak release rate, or leave it <see langword="null"/>.</param>
    /// <returns>The created <see cref="UpdateUserSettingsRequest"/>.</returns>
    public UpdateUserSettingsRequest Create(
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
        // The high threshold is generated from the medium one, so a randomized request always describes a valid meter.
        double? mediumThreshold = includeVuMeterMediumThreshold
            ? vuMeterMediumThreshold ?? _faker.Random.Double(0, 1)
            : null;
        double? highThreshold = includeVuMeterHighThreshold
            ? vuMeterHighThreshold ?? (mediumThreshold is null ? _faker.Random.Double(0, 1) : _faker.Random.Double(mediumThreshold.Value, 1))
            : null;
        return new UpdateUserSettingsRequest(
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
            includeVuMeterOrientation ? vuMeterOrientation ?? _faker.PickRandom<VuMeterOrientation>() : null,
            includeVuMeterSegmentCount ? vuMeterSegmentCount ?? _faker.Random.Int(1, 128) : null,
            includeVuMeterSegmentGap ? vuMeterSegmentGap ?? _faker.Random.Int(0, 32) : null,
            mediumThreshold,
            highThreshold,
            includeVuMeterShowPeak ? vuMeterShowPeak ?? _faker.Random.Bool() : null,
            includeVuMeterPeakHoldDurationInMilliseconds ? vuMeterPeakHoldDurationInMilliseconds ?? _faker.Random.Int(0, 10000) : null,
            includeVuMeterReleasePerSecond ? vuMeterReleasePerSecond ?? _faker.Random.Double(0, 100) : null,
            includeVuMeterPeakReleasePerSecond ? vuMeterPeakReleasePerSecond ?? _faker.Random.Double(0, 100) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateUserSettingsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateUserSettingsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
