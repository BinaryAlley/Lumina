#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryMediaItemDeletedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMediaItemDeletedDomainEventTests
{
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();
    private readonly LibraryMediaItemDeletedDomainEventFixture _libraryMediaItemDeletedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        string path = "C:/Media/Books/old-book.pdf";
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(id, libraryId, compositeId, path, occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(libraryId, domainEvent.LibraryId);
        Assert.Equal(compositeId, domainEvent.MediaLibraryScanCompositeId);
        Assert.Equal(path, domainEvent.Path);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }
}
