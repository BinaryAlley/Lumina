#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Events;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;

/// <summary>
/// Domain event raised when a playlist is deleted.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="PlaylistId">The unique identifier of the playlist.</param>
/// <param name="UserId">The unique identifier of the user that owned the playlist.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId.Value}; UserId: {UserId.Value}")]
public record PlaylistDeletedDomainEvent(
    Guid Id,
    PlaylistId PlaylistId,
    UserId UserId,
    DateTime OccurredOnUtc
) : IDomainEvent;
