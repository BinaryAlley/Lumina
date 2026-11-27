#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.Favorites;

/// <summary>
/// Data transfer object for the generic display data of a favorited media item.
/// </summary>
/// <remarks>
/// For a music artist the title is the artist name and there is no collection or creator. For a music album the title is the album title,
/// there is no collection, and the creator is the artist. For a music track the title is the track title, the collection is the album and
/// the creator is the artist. For a book the title is the book title, there is no collection yet because the series of a book is not catalogued, and the creator is its
/// first author.
/// </remarks>
/// <param name="LibraryId">The Id of the media library that contains the media item.</param>
/// <param name="Title">The title of the media item.</param>
/// <param name="CollectionId">The Id of the collection that contains the media item, or an empty Guid when there is none.</param>
/// <param name="CollectionTitle">The title of the collection that contains the media item, or an empty string when there is none.</param>
/// <param name="CreatorId">The Id of the creator of the media item, or an empty Guid when there is none.</param>
/// <param name="CreatorName">The name of the creator of the media item, or an empty string when there is none.</param>
/// <param name="CoverPath">The path of the cover art of the media item, if available.</param>
[DebuggerDisplay("LibraryId: {LibraryId}; Title: {Title}")]
public sealed record FavoriteItemDisplayDto(
    Guid LibraryId,
    string Title,
    Guid CollectionId,
    string CollectionTitle,
    Guid CreatorId,
    string CreatorName,
    string? CoverPath
);
