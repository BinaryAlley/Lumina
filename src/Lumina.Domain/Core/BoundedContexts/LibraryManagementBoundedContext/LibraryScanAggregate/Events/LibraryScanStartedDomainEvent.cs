#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Domain event raised when a libary scan is started.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="LibraryScan">The library scan that was started.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {LibraryScan.Id}")]
public record LibraryScanStartedDomainEvent(
    Guid Id,
    LibraryScan LibraryScan,
    DateTime OccurredOnUtc
) : IDomainEvent;
