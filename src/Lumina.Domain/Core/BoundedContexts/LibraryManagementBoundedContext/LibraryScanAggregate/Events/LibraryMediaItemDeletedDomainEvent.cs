#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Domain event raised when a media library item was present in the media library scan snapshot of a previous scan, but it is no longer present on disk.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="LibraryId">The unique identifier of the library to which the deleted media library item belonged.</param>
/// <param name="MediaLibraryScanCompositeId">Model for tracking media library scans.</param>
/// <param name="Path">The path of the media library item that was deleted.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {Id}; Path: {Path}")]
public record LibraryMediaItemDeletedDomainEvent(
    Guid Id,
    LibraryId LibraryId,
    MediaLibraryScanCompositeId MediaLibraryScanCompositeId,
    string Path,
    DateTime OccurredOnUtc
) : IDomainEvent;
