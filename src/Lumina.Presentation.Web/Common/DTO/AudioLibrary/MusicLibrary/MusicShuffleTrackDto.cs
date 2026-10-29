#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a track returned by a randomized shuffle query.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class MusicShuffleTrackDto
{
    /// <summary>
    /// Gets or sets the Id of the track.
    /// </summary>
    public Guid TrackId { get; set; }

    /// <summary>
    /// Gets or sets the title of the track.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration of the track in seconds.
    /// </summary>
    public int DurationInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the Id of the album the track belongs to.
    /// </summary>
    public Guid AlbumId { get; set; }

    /// <summary>
    /// Gets or sets the title of the album the track belongs to.
    /// </summary>
    public string AlbumTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Id of the artist the album belongs to.
    /// </summary>
    public Guid ArtistId { get; set; }

    /// <summary>
    /// Gets or sets the path of the cover of the album, if available.
    /// </summary>
    public string? CoverPath { get; set; }
}
