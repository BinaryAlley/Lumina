#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a music album that the user deleted and can restore.
/// </summary>
/// <param name="Id">The unique identifier of the album.</param>
/// <param name="LibraryId">The unique identifier of the media library the album belongs to.</param>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to.</param>
/// <param name="ArtistName">The name of the artist the album belongs to.</param>
/// <param name="Title">The title of the album.</param>
/// <param name="DeletedOnUtc">The date and time when the album was deleted.</param>
/// <param name="CoverPath">The relative path of the stored cover image of the album, when it has one.</param>
public record DeletedAlbumResponse(
    Guid Id,
    Guid LibraryId,
    Guid ArtistId,
    string ArtistName,
    string Title,
    DateTime DeletedOnUtc,
    string? CoverPath
);
