#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="LibraryScanFinishedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanFinishedDomainEventFixture
{
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryScanFinishedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="mediaLibraryScanCompositeId">Optional. The composite identifier of the media library scan that was finished.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="LibraryScanFinishedDomainEvent"/>.</returns>
    public LibraryScanFinishedDomainEvent Create(
        Guid? id = null,
        MediaLibraryScanCompositeId? mediaLibraryScanCompositeId = null,
        DateTime? occurredOnUtc = null)
    {
        return new LibraryScanFinishedDomainEvent(
            id ?? Guid.NewGuid(),
            mediaLibraryScanCompositeId ?? _mediaLibraryScanCompositeIdFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
