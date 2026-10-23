namespace Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;

/// <summary>
/// Enumeration for the kinds of media items.
/// </summary>
public enum MediaKind
{
    /// <summary>
    /// No media kind. Used as the default value; it is not a valid kind for a media item.
    /// </summary>
    None = 0,

    /// <summary>
    /// A track of a music library.
    /// </summary>
    MusicTrack,

    /// <summary>
    /// An episode of a podcast.
    /// </summary>
    PodcastEpisode,

    /// <summary>
    /// An audiobook.
    /// </summary>
    Audiobook,

    /// <summary>
    /// A movie.
    /// </summary>
    Movie,

    /// <summary>
    /// An episode of a TV show.
    /// </summary>
    TvEpisode,

    /// <summary>
    /// A book of a written content library.
    /// </summary>
    Book,

    /// <summary>
    /// A photo.
    /// </summary>
    Photo,

    /// <summary>
    /// A video file.
    /// </summary>
    Video
}
