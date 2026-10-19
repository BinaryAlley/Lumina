#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.Events;

/// <summary>
/// Domain event raised when a music play starts.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="MusicPlayRecordId">The unique identifier of the play record.</param>
/// <param name="UserId">The unique identifier of the user that started the play.</param>
/// <param name="TrackId">The unique identifier of the track that was played.</param>
/// <param name="LibraryId">The unique identifier of the media library that contains the track.</param>
/// <param name="AlbumId">The unique identifier of the album that contains the track.</param>
/// <param name="ArtistId">The unique identifier of the artist that produced the track.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("MusicPlayRecordId: {MusicPlayRecordId.Value}; TrackId: {TrackId.Value}")]
public record MusicPlayStartedDomainEvent(
    Guid Id,
    MusicPlayRecordId MusicPlayRecordId,
    UserId UserId,
    TrackId TrackId,
    LibraryId LibraryId,
    AlbumId AlbumId,
    ArtistId ArtistId,
    DateTime OccurredOnUtc
) : IDomainEvent;
