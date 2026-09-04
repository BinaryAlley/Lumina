#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanFinishedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanFinishedDomainEventTests
{
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();
    private readonly LibraryScanFinishedDomainEventFixture _libraryScanFinishedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        LibraryScanFinishedDomainEvent domainEvent = _libraryScanFinishedDomainEventFixture.Create(id, compositeId, occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(compositeId, domainEvent.MediaLibraryScanCompositeId);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
