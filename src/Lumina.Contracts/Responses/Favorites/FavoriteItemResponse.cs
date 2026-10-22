#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Favorites;

/// <summary>
/// Represents a response for a favorite of the current user together with the display details of the media item it references.
/// </summary>
/// <remarks>
/// The display fields are generic across media kinds. For a music artist the title is the artist name and there is no collection or
/// creator. For a music album the creator is the artist and there is no collection. For a music track the collection is the album and the
/// creator is the artist. For a book there is no collection yet, because the series of a book is not catalogued, and the creator is its first author.
/// </remarks>
/// <param name="MediaKind">The kind of the favorited media item.</param>
/// <param name="MediaItemId">The Id of the favorited media item.</param>
/// <param name="IsAvailable">Whether the media item still exists and can be accessed by the user. When <see langword="false"/>, every display field is empty.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item, or an empty Guid when the item is not available.</param>
/// <param name="Title">The title of the media item, or an empty string when the item is not available.</param>
/// <param name="CollectionId">The Id of the collection that contains the media item, or an empty Guid when there is none.</param>
/// <param name="CollectionTitle">The title of the collection that contains the media item, or an empty string when there is none.</param>
/// <param name="CreatorId">The Id of the creator of the media item, or an empty Guid when there is none.</param>
/// <param name="CreatorName">The name of the creator of the media item, or an empty string when there is none.</param>
/// <param name="CoverPath">The path of the cover art of the media item, if available.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public sealed record FavoriteItemResponse(
    FavoriteMediaKind MediaKind,
    Guid MediaItemId,
    bool IsAvailable,
    Guid LibraryId,
    string Title,
    Guid CollectionId,
    string CollectionTitle,
    Guid CreatorId,
    string CreatorName,
    string? CoverPath
);
