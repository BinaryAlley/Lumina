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
/// Fixture class for the <see cref="LibraryScanFailedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanFailedDomainEventFixture
{
    private readonly Faker _faker = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryScanFailedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="libraryId">Optional. The unique identifier of the library whose scan has failed.</param>
    /// <param name="mediaLibraryScanCompositeId">Optional. The composite identifier of the media library scan that has failed.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <param name="errorMessage">Optional. The descriptive message of the failure.</param>
    /// <param name="includeErrorMessage">Whether to include an error message in the domain event. When false, the error message is left unset.</param>
    /// <returns>The created <see cref="LibraryScanFailedDomainEvent"/>.</returns>
    public LibraryScanFailedDomainEvent Create(
        Guid? id = null,
        LibraryId? libraryId = null,
        MediaLibraryScanCompositeId? mediaLibraryScanCompositeId = null,
        DateTime? occurredOnUtc = null,
        string? errorMessage = null,
        bool includeErrorMessage = true)
    {
        return new LibraryScanFailedDomainEvent(
            id ?? Guid.NewGuid(),
            libraryId ?? _libraryIdFixture.Create(),
            mediaLibraryScanCompositeId ?? _mediaLibraryScanCompositeIdFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow,
            includeErrorMessage ? (errorMessage ?? _faker.Random.Words()) : null);
    }
}
