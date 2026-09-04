#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="LibraryScanQueuedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanQueuedDomainEventFixture
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryScanQueuedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="scanId">Optional. The unique identifier of the queued media library scan.</param>
    /// <param name="libraryId">Optional. The unique identifier of the library whose scan was queued.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="LibraryScanQueuedDomainEvent"/>.</returns>
    public LibraryScanQueuedDomainEvent Create(
        Guid? id = null,
        ScanId? scanId = null,
        LibraryId? libraryId = null,
        DateTime? occurredOnUtc = null)
    {
        return new LibraryScanQueuedDomainEvent(
            id ?? Guid.NewGuid(),
            scanId ?? _scanIdFixture.Create(),
            libraryId ?? _libraryIdFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
