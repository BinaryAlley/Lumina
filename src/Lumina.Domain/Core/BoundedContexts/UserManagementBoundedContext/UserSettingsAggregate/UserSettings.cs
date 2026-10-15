#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;

/// <summary>
/// Aggregate root for the settings of a user.
/// </summary>
[DebuggerDisplay("Id: {Id}; UserId: {UserId}")]
public class UserSettings : AggregateRoot<UserSettingsId>
{
    private const bool DEFAULT_IS_PAGINATION_ENABLED = true;
    private const int DEFAULT_ITEMS_PER_PAGE = 48;
    private const bool DEFAULT_IGNORE_THE_PREFIX_FOR_ALPHA_PICKER = false;
    private const bool DEFAULT_IS_THEME_CACHING_ENABLED = true;
    private const bool DEFAULT_AGGREGATE_METADATA_WHEN_MISSING = false;
    private const bool DEFAULT_AGGREGATE_ARTWORK_WHEN_MISSING = false;
    private const bool DEFAULT_SHOULD_RENDER_PDF_AS_IMAGES = false;
    private const bool DEFAULT_SHOULD_PRESERVE_BOOK_STYLES = true;
    private const bool DEFAULT_SHOULD_KEEP_DOWNLOADING_WHILE_PAUSED = true;
    private const bool DEFAULT_SHOULD_AUTOPLAY = true;
    private const bool DEFAULT_IS_GAPLESS_PLAYBACK_ENABLED = true;
    private const ReplayGainMode DEFAULT_REPLAY_GAIN_MODE = ReplayGainMode.Off;
    private const PlaybackStreamingQuality DEFAULT_STREAMING_QUALITY = PlaybackStreamingQuality.Auto;
    private const double DEFAULT_VOLUME = 1.0;
    private const bool DEFAULT_IS_MUTED = false;

    /// <summary>
    /// Gets the object representing the unique identifier of the user that owns these settings.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets whether pagination is enabled for the user, or not.
    /// </summary>
    public bool IsPaginationEnabled { get; private set; }

    /// <summary>
    /// Gets the number of library items displayed per page when pagination is enabled.
    /// </summary>
    public int ItemsPerPage { get; private set; }

    /// <summary>
    /// Gets whether the "The" prefix of library item titles is ignored by the alpha picker, or not.
    /// </summary>
    public bool ShouldIgnoreThePrefixForAlphaPicker { get; private set; }

    /// <summary>
    /// Gets whether the theme data served to this user is cached, or not.
    /// </summary>
    public bool IsThemeCachingEnabled { get; private set; }

    /// <summary>
    /// Gets whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.
    /// </summary>
    public bool ShouldAggregateMetadataWhenMissing { get; private set; }

    /// <summary>
    /// Gets whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not.
    /// </summary>
    public bool ShouldAggregateArtworkWhenMissing { get; private set; }

    /// <summary>
    /// Gets whether PDF books are rendered as page images for the user, or not. Rendering PDFs as images preserves the original
    /// layout, but the pages are not selectable or searchable, unlike their text layer.
    /// </summary>
    public bool ShouldRenderPdfAsImages { get; private set; }

    /// <summary>
    /// Gets whether the styles of the book content are preserved when it is rendered for the user, or not. Preserving them keeps the
    /// original look of a book (its fonts, colors, and alignment), but the styles of a book can load resources from external servers,
    /// which those servers could observe; stripping them removes that possibility, at the cost of the look of the book.
    /// </summary>
    public bool ShouldPreserveBookStyles { get; private set; }

    /// <summary>
    /// Gets whether the player keeps downloading a track while playback is paused, or not, up to the entire file.
    /// </summary>
    public bool ShouldKeepDownloadingWhilePaused { get; private set; }

    /// <summary>
    /// Gets whether the player continues playback at the end of an album or artist, or not.
    /// </summary>
    public bool ShouldAutoplay { get; private set; }

    /// <summary>
    /// Gets whether the player transitions between consecutive tracks without a gap, or not.
    /// </summary>
    public bool IsGaplessPlaybackEnabled { get; private set; }

    /// <summary>
    /// Gets the way ReplayGain normalization is applied to the tracks the user plays.
    /// </summary>
    public ReplayGainMode ReplayGainMode { get; private set; }

    /// <summary>
    /// Gets the quality cap applied to the audio the server normalizes for the user.
    /// </summary>
    public PlaybackStreamingQuality StreamingQuality { get; private set; }

    /// <summary>
    /// Gets the last volume the player reported for the user. It is written automatically by the player, and is not part of the settings the user edits.
    /// </summary>
    public double Volume { get; private set; }

    /// <summary>
    /// Gets the last mute state the player reported for the user. It is written automatically by the player, and is not part of the settings the user edits.
    /// </summary>
    public bool IsMuted { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettings"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of these settings.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not.</param>
    /// <param name="shouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user, or not.</param>
    /// <param name="shouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user, or not.</param>
    /// <param name="shouldKeepDownloadingWhilePaused">Whether the player keeps downloading a track while playback is paused, or not.</param>
    /// <param name="shouldAutoplay">Whether the player continues playback at the end of an album or artist, or not.</param>
    /// <param name="isGaplessPlaybackEnabled">Whether the player transitions between consecutive tracks without a gap, or not.</param>
    /// <param name="replayGainMode">The way ReplayGain normalization is applied to the tracks the user plays.</param>
    /// <param name="streamingQuality">The quality cap applied to the audio the server normalizes for the user.</param>
    /// <param name="volume">The last volume the player reported for the user.</param>
    /// <param name="isMuted">The last mute state the player reported for the user.</param>
    private UserSettings(
        UserSettingsId id,
        UserId userId,
        bool isPaginationEnabled,
        int itemsPerPage,
        bool shouldIgnoreThePrefixForAlphaPicker,
        bool isThemeCachingEnabled,
        bool shouldAggregateMetadataWhenMissing,
        bool shouldAggregateArtworkWhenMissing,
        bool shouldRenderPdfAsImages,
        bool shouldPreserveBookStyles,
        bool shouldKeepDownloadingWhilePaused,
        bool shouldAutoplay,
        bool isGaplessPlaybackEnabled,
        ReplayGainMode replayGainMode,
        PlaybackStreamingQuality streamingQuality,
        double volume,
        bool isMuted) : base(id)
    {
        UserId = userId;
        IsPaginationEnabled = isPaginationEnabled;
        ItemsPerPage = itemsPerPage;
        ShouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker;
        IsThemeCachingEnabled = isThemeCachingEnabled;
        ShouldAggregateMetadataWhenMissing = shouldAggregateMetadataWhenMissing;
        ShouldAggregateArtworkWhenMissing = shouldAggregateArtworkWhenMissing;
        ShouldRenderPdfAsImages = shouldRenderPdfAsImages;
        ShouldPreserveBookStyles = shouldPreserveBookStyles;
        ShouldKeepDownloadingWhilePaused = shouldKeepDownloadingWhilePaused;
        ShouldAutoplay = shouldAutoplay;
        IsGaplessPlaybackEnabled = isGaplessPlaybackEnabled;
        ReplayGainMode = replayGainMode;
        StreamingQuality = streamingQuality;
        Volume = volume;
        IsMuted = isMuted;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettings"/> class, with the default settings.
    /// </summary>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> Create()
    {
        return Create(
            UserId.CreateUnique(),
            DEFAULT_IS_PAGINATION_ENABLED,
            DEFAULT_ITEMS_PER_PAGE,
            DEFAULT_IGNORE_THE_PREFIX_FOR_ALPHA_PICKER,
            DEFAULT_IS_THEME_CACHING_ENABLED,
            DEFAULT_AGGREGATE_METADATA_WHEN_MISSING,
            DEFAULT_AGGREGATE_ARTWORK_WHEN_MISSING,
            DEFAULT_SHOULD_RENDER_PDF_AS_IMAGES,
            DEFAULT_SHOULD_PRESERVE_BOOK_STYLES,
            DEFAULT_SHOULD_KEEP_DOWNLOADING_WHILE_PAUSED,
            DEFAULT_SHOULD_AUTOPLAY,
            DEFAULT_IS_GAPLESS_PLAYBACK_ENABLED,
            DEFAULT_REPLAY_GAIN_MODE,
            DEFAULT_STREAMING_QUALITY,
            DEFAULT_VOLUME,
            DEFAULT_IS_MUTED
        );
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettings"/> class, with the default settings.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that owns these settings.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> Create(UserId userId)
    {
        return Create(
            userId,
            DEFAULT_IS_PAGINATION_ENABLED,
            DEFAULT_ITEMS_PER_PAGE,
            DEFAULT_IGNORE_THE_PREFIX_FOR_ALPHA_PICKER,
            DEFAULT_IS_THEME_CACHING_ENABLED,
            DEFAULT_AGGREGATE_METADATA_WHEN_MISSING,
            DEFAULT_AGGREGATE_ARTWORK_WHEN_MISSING,
            DEFAULT_SHOULD_RENDER_PDF_AS_IMAGES,
            DEFAULT_SHOULD_PRESERVE_BOOK_STYLES,
            DEFAULT_SHOULD_KEEP_DOWNLOADING_WHILE_PAUSED,
            DEFAULT_SHOULD_AUTOPLAY,
            DEFAULT_IS_GAPLESS_PLAYBACK_ENABLED,
            DEFAULT_REPLAY_GAIN_MODE,
            DEFAULT_STREAMING_QUALITY,
            DEFAULT_VOLUME,
            DEFAULT_IS_MUTED
        );
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettings"/> class.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not.</param>
    /// <param name="shouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user, or not.</param>
    /// <param name="shouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user, or not.</param>
    /// <param name="shouldKeepDownloadingWhilePaused">Whether the player keeps downloading a track while playback is paused, or not.</param>
    /// <param name="shouldAutoplay">Whether the player continues playback at the end of an album or artist, or not.</param>
    /// <param name="isGaplessPlaybackEnabled">Whether the player transitions between consecutive tracks without a gap, or not.</param>
    /// <param name="replayGainMode">The way ReplayGain normalization is applied to the tracks the user plays.</param>
    /// <param name="streamingQuality">The quality cap applied to the audio the server normalizes for the user.</param>
    /// <param name="volume">The last volume the player reported for the user.</param>
    /// <param name="isMuted">The last mute state the player reported for the user.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> Create(
        UserId userId,
        bool isPaginationEnabled,
        int itemsPerPage,
        bool shouldIgnoreThePrefixForAlphaPicker,
        bool isThemeCachingEnabled,
        bool shouldAggregateMetadataWhenMissing,
        bool shouldAggregateArtworkWhenMissing,
        bool shouldRenderPdfAsImages,
        bool shouldPreserveBookStyles,
        bool shouldKeepDownloadingWhilePaused,
        bool shouldAutoplay,
        bool isGaplessPlaybackEnabled,
        ReplayGainMode replayGainMode,
        PlaybackStreamingQuality streamingQuality,
        double volume,
        bool isMuted)
    {
        return new UserSettings(
            UserSettingsId.CreateUnique(),
            userId,
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
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettings"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of these settings.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not.</param>
    /// <param name="shouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user, or not.</param>
    /// <param name="shouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user, or not.</param>
    /// <param name="shouldKeepDownloadingWhilePaused">Whether the player keeps downloading a track while playback is paused, or not.</param>
    /// <param name="shouldAutoplay">Whether the player continues playback at the end of an album or artist, or not.</param>
    /// <param name="isGaplessPlaybackEnabled">Whether the player transitions between consecutive tracks without a gap, or not.</param>
    /// <param name="replayGainMode">The way ReplayGain normalization is applied to the tracks the user plays.</param>
    /// <param name="streamingQuality">The quality cap applied to the audio the server normalizes for the user.</param>
    /// <param name="volume">The last volume the player reported for the user.</param>
    /// <param name="isMuted">The last mute state the player reported for the user.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> Create(
        UserSettingsId id,
        UserId userId,
        bool isPaginationEnabled,
        int itemsPerPage,
        bool shouldIgnoreThePrefixForAlphaPicker,
        bool isThemeCachingEnabled,
        bool shouldAggregateMetadataWhenMissing,
        bool shouldAggregateArtworkWhenMissing,
        bool shouldRenderPdfAsImages,
        bool shouldPreserveBookStyles,
        bool shouldKeepDownloadingWhilePaused,
        bool shouldAutoplay,
        bool isGaplessPlaybackEnabled,
        ReplayGainMode replayGainMode,
        PlaybackStreamingQuality streamingQuality,
        double volume,
        bool isMuted)
    {
        return new UserSettings(id, userId, isPaginationEnabled, itemsPerPage, shouldIgnoreThePrefixForAlphaPicker, isThemeCachingEnabled, shouldAggregateMetadataWhenMissing, shouldAggregateArtworkWhenMissing, shouldRenderPdfAsImages, shouldPreserveBookStyles, shouldKeepDownloadingWhilePaused, shouldAutoplay, isGaplessPlaybackEnabled, replayGainMode, streamingQuality, volume, isMuted);
    }

    /// <summary>
    /// Updates the settings of the user.
    /// </summary>
    /// <remarks>
    /// The playback state the player reports automatically (the volume and the mute state) is not part of the settings the user edits, so
    /// this method neither accepts nor changes it.
    /// </remarks>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not.</param>
    /// <param name="shouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user, or not.</param>
    /// <param name="shouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user, or not.</param>
    /// <param name="shouldKeepDownloadingWhilePaused">Whether the player keeps downloading a track while playback is paused, or not.</param>
    /// <param name="shouldAutoplay">Whether the player continues playback at the end of an album or artist, or not.</param>
    /// <param name="isGaplessPlaybackEnabled">Whether the player transitions between consecutive tracks without a gap, or not.</param>
    /// <param name="replayGainMode">The way ReplayGain normalization is applied to the tracks the user plays.</param>
    /// <param name="streamingQuality">The quality cap applied to the audio the server normalizes for the user.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> UpdateSettings(
        bool isPaginationEnabled,
        int itemsPerPage,
        bool shouldIgnoreThePrefixForAlphaPicker,
        bool isThemeCachingEnabled,
        bool shouldAggregateMetadataWhenMissing,
        bool shouldAggregateArtworkWhenMissing,
        bool shouldRenderPdfAsImages,
        bool shouldPreserveBookStyles,
        bool shouldKeepDownloadingWhilePaused,
        bool shouldAutoplay,
        bool isGaplessPlaybackEnabled,
        ReplayGainMode replayGainMode,
        PlaybackStreamingQuality streamingQuality)
    {
        IsPaginationEnabled = isPaginationEnabled;
        ItemsPerPage = itemsPerPage;
        ShouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker;
        IsThemeCachingEnabled = isThemeCachingEnabled;
        ShouldAggregateMetadataWhenMissing = shouldAggregateMetadataWhenMissing;
        ShouldAggregateArtworkWhenMissing = shouldAggregateArtworkWhenMissing;
        ShouldRenderPdfAsImages = shouldRenderPdfAsImages;
        ShouldPreserveBookStyles = shouldPreserveBookStyles;
        ShouldKeepDownloadingWhilePaused = shouldKeepDownloadingWhilePaused;
        ShouldAutoplay = shouldAutoplay;
        IsGaplessPlaybackEnabled = isGaplessPlaybackEnabled;
        ReplayGainMode = replayGainMode;
        StreamingQuality = streamingQuality;
        return Result.Updated;
    }

    /// <summary>
    /// Updates the playback state the player reports automatically.
    /// </summary>
    /// <param name="volume">The last volume the player reported for the user.</param>
    /// <param name="isMuted">The last mute state the player reported for the user.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> UpdatePlaybackState(double volume, bool isMuted)
    {
        Volume = volume;
        IsMuted = isMuted;
        return Result.Updated;
    }
}
