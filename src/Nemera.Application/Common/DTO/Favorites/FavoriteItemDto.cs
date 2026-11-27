#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.Favorites;

/// <summary>
/// Data transfer object for a stored favorite together with the display data of the media item it references.
/// </summary>
/// <param name="Favorite">The stored favorite.</param>
/// <param name="Display">The display data of the referenced media item, or <see langword="null"/> when it no longer exists.</param>
[DebuggerDisplay("Id: {Favorite.Id}; MediaKind: {Favorite.MediaKind}")]
public sealed record FavoriteItemDto(
    FavoriteEntity Favorite,
    FavoriteItemDisplayDto? Display
);
