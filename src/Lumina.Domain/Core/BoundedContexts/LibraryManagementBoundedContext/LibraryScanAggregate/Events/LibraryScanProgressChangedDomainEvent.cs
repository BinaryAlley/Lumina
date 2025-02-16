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
/// <param name="ScanId">The unique identifier of the failed media library scan.</param>
/// <param name="LibraryId">The unique identifier of the library whose scan progress changes.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {Id}")]
public record LibraryScanProgressChangedDomainEvent(
    Guid Id,
    ScanId ScanId,
    LibraryId LibraryId,
    DateTime OccurredOnUtc
) : IDomainEvent;
