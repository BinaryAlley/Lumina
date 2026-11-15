#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a music album that the user deleted and can restore.
/// </summary>
/// <param name="Id">The Id of the deleted album.</param>
/// <param name="LibraryId">The Id of the media library the deleted album belongs to.</param>
/// <param name="ArtistId">The Id of the artist the deleted album belongs to.</param>
/// <param name="ArtistName">The name of the artist the deleted album belongs to.</param>
/// <param name="Title">The title of the deleted album.</param>
/// <param name="DeletedOnUtc">The date and time when the album was deleted.</param>
/// <param name="CoverPath">The relative path of the stored cover image of the deleted album, when it has one.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record DeletedMusicAlbumDto(
    Guid Id,
    Guid LibraryId,
    Guid ArtistId,
    string ArtistName,
    string Title,
    DateTime DeletedOnUtc,
    string? CoverPath
);
