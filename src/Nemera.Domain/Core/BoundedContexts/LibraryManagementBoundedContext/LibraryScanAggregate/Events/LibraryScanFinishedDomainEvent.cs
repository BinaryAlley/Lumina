#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Domain event raised when a libary scan is finished.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="MediaLibraryScanCompositeId">Model for tracking media library scans.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("ScanId: {ScanId}; UserId: {UserId}")]
public record LibraryScanFinishedDomainEvent(
    Guid Id,
    MediaLibraryScanCompositeId MediaLibraryScanCompositeId,
    DateTime OccurredOnUtc
) : IDomainEvent;
