#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="LibrarySavedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibrarySavedDomainEventFixture
{
    private readonly LibraryFixture _libraryFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibrarySavedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="library">Optional. The library that was created or updated.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="LibrarySavedDomainEvent"/>.</returns>
    public LibrarySavedDomainEvent Create(Guid? id = null, Library? library = null, DateTime? occurredOnUtc = null)
    {
        return new LibrarySavedDomainEvent(
            id ?? Guid.NewGuid(),
            library ?? _libraryFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
