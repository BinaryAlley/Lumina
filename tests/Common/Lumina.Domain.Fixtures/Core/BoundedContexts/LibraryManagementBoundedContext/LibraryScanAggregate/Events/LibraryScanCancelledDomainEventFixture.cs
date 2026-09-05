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
/// Fixture class for the <see cref="LibraryScanCancelledDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanCancelledDomainEventFixture
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryScanCancelledDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="scanId">Optional. The unique identifier of the cancelled media library scan.</param>
    /// <param name="libraryId">Optional. The unique identifier of the library whose scan was cancelled.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="LibraryScanCancelledDomainEvent"/>.</returns>
    public LibraryScanCancelledDomainEvent Create(
        Guid? id = null,
        ScanId? scanId = null,
        LibraryId? libraryId = null,
        DateTime? occurredOnUtc = null)
    {
        return new LibraryScanCancelledDomainEvent(
            id ?? Guid.NewGuid(),
            scanId ?? _scanIdFixture.Create(),
            libraryId ?? _libraryIdFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
