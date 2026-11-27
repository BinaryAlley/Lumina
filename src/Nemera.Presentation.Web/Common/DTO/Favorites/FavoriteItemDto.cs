#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Favorites;

/// <summary>
/// Data transfer object for a favorite of the current user together with the display details of the media item it references.
/// </summary>
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
public sealed record FavoriteItemDto(
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
