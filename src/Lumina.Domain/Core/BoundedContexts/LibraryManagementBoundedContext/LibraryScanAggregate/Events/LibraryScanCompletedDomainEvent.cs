#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Domain event raised when a libary scan is completed.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="LibraryId">The unique identifier of the library whose scan was completed.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {Id}")]
public record LibraryScanCompletedDomainEvent(
    Guid Id,
    LibraryId LibraryId,
    DateTime OccurredOnUtc
) : IDomainEvent;
