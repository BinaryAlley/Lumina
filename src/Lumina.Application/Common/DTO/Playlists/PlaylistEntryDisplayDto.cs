#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for the display data of the media item referenced by a playlist entry, together with its collection and creator.
/// </summary>
/// <param name="TrackId">The Id of the track.</param>
/// <param name="TrackTitle">The title of the track.</param>
/// <param name="DurationInSeconds">The duration of the track in seconds.</param>
/// <param name="LibraryId">The Id of the media library that contains the track.</param>
/// <param name="AlbumId">The Id of the album that contains the track.</param>
/// <param name="AlbumTitle">The title of the album that contains the track.</param>
/// <param name="ArtistId">The Id of the artist that produced the track.</param>
/// <param name="ArtistName">The name of the artist that produced the track.</param>
/// <param name="CoverPath">The path of the cover art of the album, if available.</param>
[DebuggerDisplay("TrackId: {TrackId}; TrackTitle: {TrackTitle}")]
public sealed record PlaylistEntryDisplayDto(
    Guid TrackId,
    string TrackTitle,
    int DurationInSeconds,
    Guid LibraryId,
    Guid AlbumId,
    string AlbumTitle,
    Guid ArtistId,
    string ArtistName,
    string? CoverPath
);
