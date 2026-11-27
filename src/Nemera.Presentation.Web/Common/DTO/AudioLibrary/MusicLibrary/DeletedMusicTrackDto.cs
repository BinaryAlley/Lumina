#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a music track that the user deleted and can restore.
/// </summary>
/// <param name="Id">The Id of the deleted track.</param>
/// <param name="LibraryId">The Id of the media library the deleted track belongs to.</param>
/// <param name="ArtistId">The Id of the artist the deleted track belongs to.</param>
/// <param name="ArtistName">The name of the artist the deleted track belongs to.</param>
/// <param name="AlbumId">The Id of the album the deleted track belongs to.</param>
/// <param name="AlbumTitle">The title of the album the deleted track belongs to.</param>
/// <param name="Title">The title of the deleted track.</param>
/// <param name="Path">The file system path of the deleted track.</param>
/// <param name="DeletedOnUtc">The date and time when the track was deleted.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record DeletedMusicTrackDto(
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
