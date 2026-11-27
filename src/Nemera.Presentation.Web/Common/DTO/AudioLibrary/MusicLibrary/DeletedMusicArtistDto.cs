#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a music artist that the user deleted and can restore.
/// </summary>
/// <param name="Id">The Id of the deleted artist.</param>
/// <param name="LibraryId">The Id of the media library the deleted artist belongs to.</param>
/// <param name="Name">The name of the deleted artist.</param>
/// <param name="DeletedOnUtc">The date and time when the artist was deleted.</param>
/// <param name="CoverPath">The relative path of the stored cover image of the deleted artist, when it has one.</param>
[DebuggerDisplay("Name: {Name}")]
public sealed record DeletedMusicArtistDto(
    Guid Id,
    Guid LibraryId,
    string Name,
    DateTime DeletedOnUtc,
    string? CoverPath
);
