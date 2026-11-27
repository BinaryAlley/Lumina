#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a music artist that the user deleted and can restore.
/// </summary>
/// <param name="Id">The unique identifier of the artist.</param>
/// <param name="LibraryId">The unique identifier of the media library the artist belongs to.</param>
/// <param name="Name">The name of the artist.</param>
/// <param name="DeletedOnUtc">The date and time when the artist was deleted.</param>
/// <param name="CoverPath">The relative path of the stored cover image of the artist, when it has one.</param>
public record DeletedArtistResponse(
    Guid Id,
    Guid LibraryId,
    string Name,
    DateTime DeletedOnUtc,
    string? CoverPath
);
