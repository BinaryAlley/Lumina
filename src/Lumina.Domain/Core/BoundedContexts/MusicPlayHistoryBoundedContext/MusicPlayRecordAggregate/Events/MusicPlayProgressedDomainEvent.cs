#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.Events;

/// <summary>
/// Domain event raised when a player reports the progress of a music play.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="MusicPlayRecordId">The unique identifier of the play record.</param>
/// <param name="UserId">The unique identifier of the user that owns the play record.</param>
/// <param name="TrackId">The unique identifier of the track that is being played.</param>
/// <param name="Position">The last position reported by the player.</param>
/// <param name="ListenedDuration">The duration that the user has listened to so far.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("MusicPlayRecordId: {MusicPlayRecordId.Value}; Position: {Position.Offset}")]
public record MusicPlayProgressedDomainEvent(
    Guid Id,
    MusicPlayRecordId MusicPlayRecordId,
    UserId UserId,
    TrackId TrackId,
    MusicPlayPosition Position,
    TimeSpan ListenedDuration,
    DateTime OccurredOnUtc
) : IDomainEvent;
