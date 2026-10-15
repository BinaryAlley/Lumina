#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.UsersManagement.Settings;

/// <summary>
/// Represents a request to update the settings of the current user.
/// </summary>
/// <param name="IsPaginationEnabled">Whether pagination is enabled for the user, or not. Required.</param>
/// <param name="ItemsPerPage">The number of library items displayed per page when pagination is enabled. Required.</param>
/// <param name="ShouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not. Required.</param>
/// <param name="IsThemeCachingEnabled">Whether the theme data served to this user is cached, or not. Required.</param>
/// <param name="ShouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not. Required.</param>
/// <param name="ShouldAggregateArtworkWhenMissing">Whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not. Required.</param>
/// <param name="ShouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user, or not. Required.</param>
/// <param name="ShouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user, or not. Required.</param>
/// <param name="ShouldKeepDownloadingWhilePaused">Whether the player keeps downloading a track while playback is paused, or not. Required.</param>
/// <param name="ShouldAutoplay">Whether the player continues playback at the end of an album or artist, or not. Required.</param>
/// <param name="IsGaplessPlaybackEnabled">Whether the player transitions between consecutive tracks without a gap, or not. Required.</param>
/// <param name="ReplayGainMode">The way ReplayGain normalization is applied to the tracks the user plays. Required.</param>
/// <param name="StreamingQuality">The quality cap applied to the audio the server normalizes for the user. Required.</param>
[DebuggerDisplay("IsPaginationEnabled: {IsPaginationEnabled}; ItemsPerPage: {ItemsPerPage}")]
public sealed record UpdateUserSettingsRequest(
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
    PlaybackStreamingQuality? StreamingQuality
);
