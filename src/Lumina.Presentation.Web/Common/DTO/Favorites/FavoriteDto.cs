#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Favorites;

/// <summary>
/// Data transfer object for a favorite of the current user.
/// </summary>
/// <param name="Id">The Id of the favorite.</param>
/// <param name="MediaKind">The kind of the favorited media item.</param>
/// <param name="MediaItemId">The Id of the favorited media item.</param>
/// <param name="FavoritedOnUtc">The date and time when the media item was favorited.</param>
[DebuggerDisplay("Id: {Id}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public sealed record FavoriteDto(
    Guid Id,
    FavoriteMediaKind MediaKind,
    Guid MediaItemId,
    DateTime FavoritedOnUtc
);
