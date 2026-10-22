#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Favorites;

/// <summary>
/// Represents a response that tells which of the checked media items are favorites of the current user.
/// </summary>
/// <param name="FavoritedMediaItemIds">The Ids, among the checked ones, of the media items that are favorites of the current user.</param>
[DebuggerDisplay("Favorited: {FavoritedMediaItemIds.Count}")]
public sealed record FavoriteStatusResponse(
    IReadOnlyList<Guid> FavoritedMediaItemIds
);
