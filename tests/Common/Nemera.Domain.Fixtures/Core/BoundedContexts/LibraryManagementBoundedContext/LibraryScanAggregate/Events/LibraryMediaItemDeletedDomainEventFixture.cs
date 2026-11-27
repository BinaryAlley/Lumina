#region ========================================================================= USING =====================================================================================
using Bogus;
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
/// Fixture class for the <see cref="LibraryMediaItemDeletedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMediaItemDeletedDomainEventFixture
{
    private readonly Faker _faker = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryMediaItemDeletedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="libraryId">Optional. The unique identifier of the library to which the deleted media library item belonged.</param>
    /// <param name="mediaLibraryScanCompositeId">Optional. The composite identifier of the media library scan during which the item deletion was detected.</param>
    /// <param name="path">Optional. The path of the media library item that was deleted.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="LibraryMediaItemDeletedDomainEvent"/>.</returns>
    public LibraryMediaItemDeletedDomainEvent Create(
        Guid? id = null,
        LibraryId? libraryId = null,
        MediaLibraryScanCompositeId? mediaLibraryScanCompositeId = null,
        string? path = null,
        DateTime? occurredOnUtc = null)
    {
        return new LibraryMediaItemDeletedDomainEvent(
            id ?? Guid.NewGuid(),
            libraryId ?? _libraryIdFixture.Create(),
            mediaLibraryScanCompositeId ?? _mediaLibraryScanCompositeIdFixture.Create(),
            path ?? _faker.System.FilePath(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
