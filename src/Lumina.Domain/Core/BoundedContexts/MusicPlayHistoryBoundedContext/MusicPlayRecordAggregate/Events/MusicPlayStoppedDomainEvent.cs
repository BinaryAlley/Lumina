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
/// Domain event raised when a music play stops before the track completes.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="MusicPlayRecordId">The unique identifier of the play record.</param>
/// <param name="UserId">The unique identifier of the user that owns the play record.</param>
/// <param name="TrackId">The unique identifier of the track that was stopped.</param>
/// <param name="Position">The last position reported by the player.</param>
/// <param name="ListenedDuration">The duration that the user listened to.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("MusicPlayRecordId: {MusicPlayRecordId.Value}; TrackId: {TrackId.Value}")]
public record MusicPlayStoppedDomainEvent(
    Guid Id,
    MusicPlayRecordId MusicPlayRecordId,
    UserId UserId,
    TrackId TrackId,
    MusicPlayPosition Position,
    TimeSpan ListenedDuration,
    DateTime OccurredOnUtc
) : IDomainEvent;
