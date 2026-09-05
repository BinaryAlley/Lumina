#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="LibraryScanStartedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanStartedDomainEventFixture
{
    private readonly LibraryScanFixture _libraryScanFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryScanStartedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="libraryScan">Optional. The library scan that was started.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="LibraryScanStartedDomainEvent"/>.</returns>
    public LibraryScanStartedDomainEvent Create(Guid? id = null, LibraryScan? libraryScan = null, DateTime? occurredOnUtc = null)
    {
        return new LibraryScanStartedDomainEvent(
            id ?? Guid.NewGuid(),
            libraryScan ?? _libraryScanFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
