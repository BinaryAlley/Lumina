#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Domain event raised when a libary scan is queued.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="ScanId">The unique identifier of the queued media library scan.</param>
/// <param name="UserId">The unique identifier of the user that initiated the queued media library scan.</param>
/// <param name="LibraryId">The unique identifier of the library whose scan was queued.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {Id}")]
public record LibraryScanQueuedDomainEvent(
    Guid Id,
    ScanId ScanId,
    UserId UserId,
    LibraryId LibraryId,
    DateTime OccurredOnUtc
) : IDomainEvent;
