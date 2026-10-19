#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a response for a track returned by a randomized shuffle query, carrying everything the client needs to queue and play it.
/// </summary>
/// <param name="TrackId">The Id of the track.</param>
/// <param name="Title">The title of the track.</param>
/// <param name="DurationInSeconds">The duration of the track in seconds.</param>
/// <param name="AlbumId">The Id of the album the track belongs to.</param>
/// <param name="AlbumTitle">The title of the album the track belongs to.</param>
/// <param name="ArtistId">The Id of the artist the album belongs to.</param>
/// <param name="CoverPath">The path of the cover of the album, if available.</param>
[DebuggerDisplay("Title: {Title}")]
public record ShuffleTrackResponse(
    Guid TrackId,
    string Title,
    int DurationInSeconds,
    Guid AlbumId,
    string AlbumTitle,
    Guid ArtistId,
    string? CoverPath
);
