#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;

/// <summary>
/// Fixture class for the <see cref="UserSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="UserSettingsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional identifier of the user that owns the settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">Number of library items displayed per page.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix is ignored by the alpha picker.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing.</param>
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
    /// <returns>A configured <see cref="UserSettingsDto"/> instance.</returns>
    public UserSettingsDto Create(
        Guid? userId = null, 
        bool? isPaginationEnabled = null, 
        int? itemsPerPage = null, 
        bool? shouldIgnoreThePrefixForAlphaPicker = null, 
        bool? isThemeCachingEnabled = null, 
        bool? shouldAggregateMetadataWhenMissing = null,
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
        // The high threshold is generated from the medium one, so a randomized meter always describes a valid meter.
        double? mediumThreshold = includeVuMeterMediumThreshold
            ? vuMeterMediumThreshold ?? _faker.Random.Double(0, 1)
            : null;
        double? highThreshold = includeVuMeterHighThreshold
            ? vuMeterHighThreshold ?? (mediumThreshold is null ? _faker.Random.Double(0, 1) : _faker.Random.Double(mediumThreshold.Value, 1))
            : null;
        return new UserSettingsDto
        {
            UserId = userId ?? Guid.NewGuid(),
            IsPaginationEnabled = isPaginationEnabled ?? _faker.Random.Bool(),
            ItemsPerPage = itemsPerPage ?? _faker.Random.Int(1, 200),
            ShouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker ?? _faker.Random.Bool(),
            IsThemeCachingEnabled = isThemeCachingEnabled ?? _faker.Random.Bool(),
            ShouldAggregateMetadataWhenMissing = shouldAggregateMetadataWhenMissing ?? _faker.Random.Bool(),
            ShouldRenderPdfAsImages = shouldRenderPdfAsImages ?? _faker.Random.Bool(),
            ShouldPreserveBookStyles = shouldPreserveBookStyles ?? _faker.Random.Bool(),
            ShouldKeepDownloadingWhilePaused = shouldKeepDownloadingWhilePaused ?? _faker.Random.Bool(),
            ShouldAutoplay = shouldAutoplay ?? _faker.Random.Bool(),
            IsGaplessPlaybackEnabled = isGaplessPlaybackEnabled ?? _faker.Random.Bool(),
            ReplayGainMode = replayGainMode ?? _faker.PickRandom<ReplayGainMode>(),
            StreamingQuality = streamingQuality ?? _faker.PickRandom<PlaybackStreamingQuality>(),
            VuMeterOrientation = includeVuMeterOrientation ? vuMeterOrientation ?? _faker.PickRandom<VuMeterOrientation>() : null,
            VuMeterSegmentCount = includeVuMeterSegmentCount ? vuMeterSegmentCount ?? _faker.Random.Int(1, 128) : null,
            VuMeterSegmentGap = includeVuMeterSegmentGap ? vuMeterSegmentGap ?? _faker.Random.Int(0, 32) : null,
            VuMeterMediumThreshold = mediumThreshold,
            VuMeterHighThreshold = highThreshold,
            VuMeterShowPeak = includeVuMeterShowPeak ? vuMeterShowPeak ?? _faker.Random.Bool() : null,
            VuMeterPeakHoldDurationInMilliseconds = includeVuMeterPeakHoldDurationInMilliseconds ? vuMeterPeakHoldDurationInMilliseconds ?? _faker.Random.Int(0, 10000) : null,
            VuMeterReleasePerSecond = includeVuMeterReleasePerSecond ? vuMeterReleasePerSecond ?? _faker.Random.Double(0, 100) : null,
            VuMeterPeakReleasePerSecond = includeVuMeterPeakReleasePerSecond ? vuMeterPeakReleasePerSecond ?? _faker.Random.Double(0, 100) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="UserSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserSettingsDto"/> instances.</returns>
    public List<UserSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
