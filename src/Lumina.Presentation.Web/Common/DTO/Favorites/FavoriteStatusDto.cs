#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Favorites;

/// <summary>
/// Data transfer object that tells which of the checked media items are favorites of the current user.
/// </summary>
/// <param name="FavoritedMediaItemIds">The Ids, among the checked ones, of the media items that are favorites of the current user.</param>
[DebuggerDisplay("Favorited: {FavoritedMediaItemIds.Count}")]
public sealed record FavoriteStatusDto(
    List<Guid> FavoritedMediaItemIds
);
