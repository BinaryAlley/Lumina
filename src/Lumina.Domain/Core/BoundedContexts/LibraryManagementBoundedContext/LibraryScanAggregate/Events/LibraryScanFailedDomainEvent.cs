#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Domain event raised when a libary scan has failed.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="LibraryId">The unique identifier of the library whose scan has failed.</param>
/// <param name="MediaLibraryScanCompositeId">Model for tracking media library scans.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {Id}")]
public record LibraryScanFailedDomainEvent(
    Guid Id,
    LibraryId LibraryId,
    MediaLibraryScanCompositeId MediaLibraryScanCompositeId,
    DateTime OccurredOnUtc
) : IDomainEvent;
