#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Read model of a track returned by a randomized shuffle query, carrying everything the client needs to queue and play it.
/// </summary>
/// <param name="TrackId">The Id of the track.</param>
/// <param name="Title">The title of the track.</param>
/// <param name="DurationInSeconds">The duration of the track in seconds.</param>
/// <param name="AlbumId">The Id of the album the track belongs to.</param>
/// <param name="AlbumTitle">The title of the album the track belongs to.</param>
/// <param name="ArtistId">The Id of the artist the album belongs to.</param>
/// <param name="CoverPath">The path of the cover of the album, if available.</param>
public sealed record ShuffleTrackRowDto(
    Guid TrackId,
    string Title,
    int DurationInSeconds,
    Guid AlbumId,
    string AlbumTitle,
    Guid ArtistId,
    string? CoverPath
);
