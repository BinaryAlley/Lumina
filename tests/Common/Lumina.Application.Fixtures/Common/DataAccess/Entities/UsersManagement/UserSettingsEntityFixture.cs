#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;

/// <summary>
/// Fixture class for the <see cref="UserSettingsEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserSettingsEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the user settings.</param>
    /// <param name="userId">Optional. The Id of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Optional. Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">Optional. The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the "The" prefix is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Optional. Whether the theme data served to this user is cached, or not.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Optional. Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.</param>
    /// <param name="shouldRenderPdfAsImages">Optional. Whether PDF books are rendered as page images for the user, or not.</param>
    /// <param name="shouldPreserveBookStyles">Optional. Whether the styles of the book content are preserved when it is rendered for the user, or not.</param>
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
    /// <param name="volume">Optional. The last volume the player reported for the user.</param>
    /// <param name="isMuted">Optional. The last mute state the player reported for the user.</param>
    /// <returns>The created user settings entity.</returns>
    public UserSettingsEntity Create(
        Guid? id = null,
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
        double? volume = null,
        bool? isMuted = null)
    {
        return new Faker<UserSettingsEntity>()
            .CustomInstantiator(f =>
            {
                // The high threshold is generated from the medium one, so a randomized entity always describes a valid meter.
                double mediumThreshold = vuMeterMediumThreshold ?? f.Random.Double(0, 1);
                return new UserSettingsEntity
                {
                    Id = id ?? Guid.NewGuid(),
                    UserId = userId ?? Guid.NewGuid(),
                    IsPaginationEnabled = isPaginationEnabled ?? f.Random.Bool(),
                    ItemsPerPage = itemsPerPage ?? f.Random.Int(1, 100),
                    ShouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker ?? f.Random.Bool(),
                    IsThemeCachingEnabled = isThemeCachingEnabled ?? f.Random.Bool(),
                    ShouldAggregateMetadataWhenMissing = shouldAggregateMetadataWhenMissing ?? f.Random.Bool(),
                    ShouldRenderPdfAsImages = shouldRenderPdfAsImages ?? f.Random.Bool(),
                    ShouldPreserveBookStyles = shouldPreserveBookStyles ?? f.Random.Bool(),
                    ShouldKeepDownloadingWhilePaused = shouldKeepDownloadingWhilePaused ?? f.Random.Bool(),
                    ShouldAutoplay = shouldAutoplay ?? f.Random.Bool(),
                    IsGaplessPlaybackEnabled = isGaplessPlaybackEnabled ?? f.Random.Bool(),
                    ReplayGainMode = replayGainMode ?? f.PickRandom<ReplayGainMode>(),
                    StreamingQuality = streamingQuality ?? f.PickRandom<PlaybackStreamingQuality>(),
                    VuMeterOrientation = vuMeterOrientation ?? f.PickRandom<VuMeterOrientation>(),
                    VuMeterSegmentCount = vuMeterSegmentCount ?? f.Random.Int(1, 128),
                    VuMeterSegmentGap = vuMeterSegmentGap ?? f.Random.Int(0, 32),
                    VuMeterMediumThreshold = mediumThreshold,
                    VuMeterHighThreshold = vuMeterHighThreshold ?? f.Random.Double(mediumThreshold, 1),
                    VuMeterShowPeak = vuMeterShowPeak ?? f.Random.Bool(),
                    VuMeterPeakHoldDurationInMilliseconds = vuMeterPeakHoldDurationInMilliseconds ?? f.Random.Int(0, 10000),
                    VuMeterReleasePerSecond = vuMeterReleasePerSecond ?? f.Random.Double(0, 100),
                    VuMeterPeakReleasePerSecond = vuMeterPeakReleasePerSecond ?? f.Random.Double(0, 100),
                    Volume = volume ?? f.Random.Double(0.0, 1.0),
                    IsMuted = isMuted ?? f.Random.Bool()
                };
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UserSettingsEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserSettingsEntity"/> instances.</returns>
    public List<UserSettingsEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
