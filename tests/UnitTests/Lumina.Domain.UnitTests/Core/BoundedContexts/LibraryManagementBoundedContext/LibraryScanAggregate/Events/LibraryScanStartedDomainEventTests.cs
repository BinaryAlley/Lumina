#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanStartedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanStartedDomainEventTests
{
    private readonly LibraryScanFixture _libraryScanFixture = new();
    private readonly LibraryScanStartedDomainEventFixture _libraryScanStartedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        LibraryScan scan = _libraryScanFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        LibraryScanStartedDomainEvent domainEvent = _libraryScanStartedDomainEventFixture.Create(id, scan, occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(scan, domainEvent.LibraryScan);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
