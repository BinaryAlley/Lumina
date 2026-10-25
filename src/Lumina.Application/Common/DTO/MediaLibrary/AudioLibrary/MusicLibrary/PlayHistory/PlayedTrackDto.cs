#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Data transfer object for a track of the listening statistics of the current user, together with its aggregated play counts.
/// </summary>
/// <param name="TrackId">The Id of the track.</param>
/// <param name="LibraryId">The Id of the media library that contains the track.</param>
/// <param name="AlbumId">The Id of the album that contains the track.</param>
/// <param name="ArtistId">The Id of the artist that produced the track.</param>
/// <param name="Title">The title of the track, if it still exists and the user can access its library.</param>
/// <param name="AlbumTitle">The title of the album that contains the track, if it still exists and the user can access its library.</param>
/// <param name="ArtistName">The name of the artist that produced the track, if it still exists and the user can access its library.</param>
/// <param name="CoverPath">The path of the cover art of the album, if available.</param>
/// <param name="LastPlayedOnUtc">The date and time of the last recorded activity of the track's most recent play.</param>
/// <param name="PlayCount">The number of plays of the track that count as plays.</param>
/// <param name="CompletedPlayCount">The number of plays of the track that completed.</param>
/// <param name="ListenedDurationMilliseconds">The duration the user listened to the track, over all of its counted plays, in milliseconds.</param>
[DebuggerDisplay("TrackId: {TrackId}; PlayCount: {PlayCount}")]
public sealed record PlayedTrackDto(
    Guid TrackId,
    Guid LibraryId,
    Guid AlbumId,
    Guid ArtistId,
    string? Title,
    string? AlbumTitle,
    string? ArtistName,
    string? CoverPath,
    DateTime LastPlayedOnUtc,
    int PlayCount,
    int CompletedPlayCount,
    long ListenedDurationMilliseconds
);
