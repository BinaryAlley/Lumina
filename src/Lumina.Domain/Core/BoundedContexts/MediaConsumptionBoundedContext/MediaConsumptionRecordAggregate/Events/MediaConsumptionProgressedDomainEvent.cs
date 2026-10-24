#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;

/// <summary>
/// Domain event raised when a client reports the progress of a media consumption session.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="MediaConsumptionRecordId">The unique identifier of the consumption record.</param>
/// <param name="UserId">The unique identifier of the user that owns the consumption record.</param>
/// <param name="LibraryId">The unique identifier of the media library that contains the consumed item.</param>
/// <param name="MediaItemId">The unique identifier of the consumed media item.</param>
/// <param name="MediaKind">The kind of media that is being consumed.</param>
/// <param name="LastPosition">The last known position for the media item, if any.</param>
/// <param name="LastProgressFraction">The last progress fraction reported by the client.</param>
/// <param name="ConsumedDuration">The active duration consumed so far.</param>
/// <param name="ConsecutivePagesRead">The number of consecutive pages read so far.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("MediaConsumptionRecordId: {MediaConsumptionRecordId.Value}; LastProgressFraction: {LastProgressFraction}")]
public record MediaConsumptionProgressedDomainEvent(
    Guid Id,
    MediaConsumptionRecordId MediaConsumptionRecordId,
    UserId UserId,
    LibraryId LibraryId,
    MediaItemId MediaItemId,
    MediaKind MediaKind,
    Optional<MediaPosition> LastPosition,
    double LastProgressFraction,
    TimeSpan ConsumedDuration,
    int ConsecutivePagesRead,
    DateTime OccurredOnUtc
) : IDomainEvent;
