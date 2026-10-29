#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Favorites;

/// <summary>
/// Represents a response for a favorite of the current user.
/// </summary>
/// <param name="Id">The Id of the favorite.</param>
/// <param name="MediaKind">The kind of the favorited media item.</param>
/// <param name="MediaItemId">The Id of the favorited media item.</param>
/// <param name="FavoritedOnUtc">The date and time when the media item was favorited.</param>
[DebuggerDisplay("Id: {Id}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public sealed record FavoriteResponse(
    Guid Id,
    FavoriteMediaKind MediaKind,
    Guid MediaItemId,
    DateTime FavoritedOnUtc
);
