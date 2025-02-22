#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Domain event raised when a libary's scan progress changes.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="LibraryId">The object representing the unique identifier of the media library being scanned.</param>
/// <param name="MediaLibraryScanCompositeId">Model for tracking media library scans.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("ScanId: {MediaLibraryScanCompositeId.ScanId}; UserId: {MediaLibraryScanCompositeId.UserId}")]
public record LibraryScanProgressChangedDomainEvent(
    Guid Id,
    LibraryId LibraryId,
    MediaLibraryScanCompositeId MediaLibraryScanCompositeId,
    DateTime OccurredOnUtc
) : IDomainEvent;
