#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Domain event raised when a libary's scan job progress changes.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="ScanId">The object representing the unique identifier of the media library scan.</param>
/// <param name="UserId">The object representing the unique identifier of the user that started the library scan whose progress changes.</param>
/// <param name="Progress">The object representing the new media library scan job progress.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("ScanId: {ScanId}; UserId: {UserId}")]
public record LibraryScanJobProgressChangedDomainEvent(
    Guid Id,
    ScanId ScanId,
    UserId UserId,
    MediaLibraryScanJobProgress Progress,
    DateTime OccurredOnUtc
) : IDomainEvent;
