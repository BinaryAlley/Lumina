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
/// Fixture class for the <see cref="LibraryScanJobProgressChangedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanJobProgressChangedDomainEventFixture
{
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();
    private readonly MediaLibraryScanJobProgressFixture _mediaLibraryScanJobProgressFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryScanJobProgressChangedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="libraryId">Optional. The unique identifier of the library being scanned.</param>
    /// <param name="mediaLibraryScanCompositeId">Optional. The composite identifier of the media library scan whose job progress changed.</param>
    /// <param name="progress">Optional. The new media library scan job progress.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="LibraryScanJobProgressChangedDomainEvent"/>.</returns>
    public LibraryScanJobProgressChangedDomainEvent Create(
        Guid? id = null,
        LibraryId? libraryId = null,
        MediaLibraryScanCompositeId? mediaLibraryScanCompositeId = null,
        MediaLibraryScanJobProgress? progress = null,
        DateTime? occurredOnUtc = null)
    {
        return new LibraryScanJobProgressChangedDomainEvent(
            id ?? Guid.NewGuid(),
            libraryId ?? _libraryIdFixture.Create(),
            mediaLibraryScanCompositeId ?? _mediaLibraryScanCompositeIdFixture.Create(),
            progress ?? _mediaLibraryScanJobProgressFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
