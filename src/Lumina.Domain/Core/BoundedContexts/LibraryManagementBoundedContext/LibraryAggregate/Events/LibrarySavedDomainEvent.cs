#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;

/// <summary>
/// Domain event raised when a libary is created or updated.
/// </summary>
/// <param name="Id">The Id of the domain event.</param>
/// <param name="Library">The library that was created or updated.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {Library.Id} Title: {Library.Title}")]
public record LibrarySavedDomainEvent(
    Guid Id,
    Library Library,
    DateTime OccurredOnUtc
) : IDomainEvent;
