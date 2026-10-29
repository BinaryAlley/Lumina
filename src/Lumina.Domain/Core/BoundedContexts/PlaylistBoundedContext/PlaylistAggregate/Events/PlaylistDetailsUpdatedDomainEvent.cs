#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;

/// <summary>
/// Domain event raised when the details of a playlist are updated.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="PlaylistId">The unique identifier of the playlist.</param>
/// <param name="UserId">The unique identifier of the user that owns the playlist.</param>
/// <param name="Name">The new name of the playlist.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId.Value}; Name: {Name}")]
public record PlaylistDetailsUpdatedDomainEvent(
    Guid Id,
    PlaylistId PlaylistId,
    UserId UserId,
    string Name,
    DateTime OccurredOnUtc
) : IDomainEvent;
