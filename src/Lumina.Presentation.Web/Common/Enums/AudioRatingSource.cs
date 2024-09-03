namespace Lumina.Presentation.Web.Common.Enums;

/// <summary>
/// Enumeration for the various sources of audio ratings.
/// </summary>
public enum AudioRatingSource
{
    /// <summary>
    /// Ratings provided by users of the Lumina media library.
    /// </summary>
    User,

    /// <summary>
    /// Ratings sourced from MusicBrainz, an open music encyclopedia that collects music metadata.
    /// </summary>
    MusicBrainz,

    /// <summary>
    /// Ratings obtained from Discogs, a database and marketplace for music recordings.
    /// </summary>
    Discogs,

    /// <summary>
    /// Ratings from Last.fm, a music website that provides music recommendations based on user listening habits.
    /// </summary>
    LastFm,
}