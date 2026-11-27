#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a music track that the user deleted and can restore.
/// </summary>
/// <param name="Id">The unique identifier of the track.</param>
/// <param name="LibraryId">The unique identifier of the media library the track belongs to.</param>
/// <param name="ArtistId">The unique identifier of the artist the track belongs to.</param>
/// <param name="ArtistName">The name of the artist the track belongs to.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to.</param>
/// <param name="AlbumTitle">The title of the album the track belongs to.</param>
/// <param name="Title">The title of the track.</param>
/// <param name="Path">The file system path of the track.</param>
/// <param name="DeletedOnUtc">The date and time when the track was deleted.</param>
public record DeletedTrackResponse(
    Guid Id,
    Guid LibraryId,
    Guid ArtistId,
    string ArtistName,
    Guid AlbumId,
    string AlbumTitle,
    string Title,
    string Path,
    DateTime DeletedOnUtc
);
