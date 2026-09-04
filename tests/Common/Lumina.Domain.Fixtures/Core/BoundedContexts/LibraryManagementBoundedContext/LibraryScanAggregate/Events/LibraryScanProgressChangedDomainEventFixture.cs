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
/// Fixture class for the <see cref="LibraryScanProgressChangedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanProgressChangedDomainEventFixture
{
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryScanProgressChangedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="libraryId">Optional. The unique identifier of the library being scanned.</param>
    /// <param name="mediaLibraryScanCompositeId">Optional. The composite identifier of the media library scan whose progress changed.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="LibraryScanProgressChangedDomainEvent"/>.</returns>
    public LibraryScanProgressChangedDomainEvent Create(
        Guid? id = null,
        LibraryId? libraryId = null,
        MediaLibraryScanCompositeId? mediaLibraryScanCompositeId = null,
        DateTime? occurredOnUtc = null)
    {
        return new LibraryScanProgressChangedDomainEvent(
            id ?? Guid.NewGuid(),
            libraryId ?? _libraryIdFixture.Create(),
            mediaLibraryScanCompositeId ?? _mediaLibraryScanCompositeIdFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
