#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibrarySavedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibrarySavedDomainEventTests
{
    private readonly LibraryFixture _libraryFixture = new();
    private readonly LibrarySavedDomainEventFixture _librarySavedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Library library = _libraryFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(id, library, occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(library, domainEvent.Library);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
