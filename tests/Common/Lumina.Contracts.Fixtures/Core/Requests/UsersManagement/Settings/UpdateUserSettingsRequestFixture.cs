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
        PlaybackStreamingQuality? streamingQuality = null)
    {
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
            streamingQuality ?? _faker.PickRandom<PlaybackStreamingQuality>());
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
