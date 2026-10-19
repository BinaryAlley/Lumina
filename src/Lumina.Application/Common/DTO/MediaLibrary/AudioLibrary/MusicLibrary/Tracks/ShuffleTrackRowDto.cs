#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Read model of a track returned by a randomized shuffle query, carrying everything the client needs to queue and play it.
/// </summary>
public sealed record ShuffleTrackRowDto
{
    /// <summary>
    /// Gets the Id of the track.
    /// </summary>
    public required Guid TrackId { get; init; }

    /// <summary>
    /// Gets the title of the track.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the duration of the track in seconds.
    /// </summary>
    public int DurationInSeconds { get; init; }

    /// <summary>
    /// Gets the Id of the album the track belongs to.
    /// </summary>
    public required Guid AlbumId { get; init; }

    /// <summary>
    /// Gets the title of the album the track belongs to.
    /// </summary>
    public required string AlbumTitle { get; init; }

    /// <summary>
    /// Gets the Id of the artist the album belongs to.
    /// </summary>
    public required Guid ArtistId { get; init; }

    /// <summary>
    /// Gets the path of the cover of the album, if available.
    /// </summary>
    public string? CoverPath { get; init; }
}
