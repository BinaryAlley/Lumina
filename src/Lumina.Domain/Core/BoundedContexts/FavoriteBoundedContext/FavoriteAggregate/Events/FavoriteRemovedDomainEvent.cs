#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;

/// <summary>
/// Domain event raised when a media item is removed from the favorites.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="FavoriteId">The unique identifier of the favorite.</param>
/// <param name="UserId">The unique identifier of the user that owned the favorite.</param>
/// <param name="MediaItemId">The unique identifier of the media item that was unfavorited.</param>
/// <param name="MediaKind">The kind of the media item that was unfavorited.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("FavoriteId: {FavoriteId.Value}; UserId: {UserId.Value}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId.Value}")]
public record FavoriteRemovedDomainEvent(
    Guid Id,
    FavoriteId FavoriteId,
    UserId UserId,
    MediaItemId MediaItemId,
    FavoriteMediaKind MediaKind,
    DateTime OccurredOnUtc
) : IDomainEvent;
