#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;

/// <summary>
/// Fixture class for the <see cref="UserSettings"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserSettings"/> domain aggregate.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user that owns the settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker.</param>
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
    /// <param name="volume">The last volume the player reported for the user.</param>
    /// <param name="isMuted">The last mute state the player reported for the user.</param>
    /// <returns>The created <see cref="UserSettings"/>.</returns>
    public UserSettings Create(
        Guid? userId = null,
        bool isPaginationEnabled = true,
        int itemsPerPage = 48,
        bool shouldIgnoreThePrefixForAlphaPicker = false,
        bool isThemeCachingEnabled = true,
        bool shouldAggregateMetadataWhenMissing = false,
        bool shouldAggregateArtworkWhenMissing = false,
        bool shouldRenderPdfAsImages = false,
        bool shouldPreserveBookStyles = true,
        bool shouldKeepDownloadingWhilePaused = true,
        bool shouldAutoplay = true,
        bool isGaplessPlaybackEnabled = true,
        ReplayGainMode replayGainMode = ReplayGainMode.Off,
        PlaybackStreamingQuality streamingQuality = PlaybackStreamingQuality.Auto,
        double volume = 1.0,
        bool isMuted = false)
    {
        Result<UserSettings> settings = UserSettings.Create(
            userId is null ? UserId.CreateUnique() : UserId.Create(userId.Value),
            isPaginationEnabled,
            itemsPerPage,
            shouldIgnoreThePrefixForAlphaPicker,
            isThemeCachingEnabled,
            shouldAggregateMetadataWhenMissing,
            shouldAggregateArtworkWhenMissing,
            shouldRenderPdfAsImages,
            shouldPreserveBookStyles,
            shouldKeepDownloadingWhilePaused,
            shouldAutoplay,
            isGaplessPlaybackEnabled,
            replayGainMode,
            streamingQuality,
            volume,
            isMuted);
        return settings.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="UserSettings"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserSettings"/> instances.</returns>
    public List<UserSettings> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
