#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;

/// <summary>
/// Domain event raised when a playlist is created.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="PlaylistId">The unique identifier of the playlist.</param>
/// <param name="UserId">The unique identifier of the user that owns the playlist.</param>
/// <param name="Name">The name of the playlist.</param>
/// <param name="Type">The kind of media the playlist holds.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId.Value}; UserId: {UserId.Value}")]
public record PlaylistCreatedDomainEvent(
    Guid Id,
    PlaylistId PlaylistId,
    UserId UserId,
    string Name,
    PlaylistType Type,
    DateTime OccurredOnUtc
) : IDomainEvent;
