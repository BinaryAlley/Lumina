#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Events;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Nemera.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanCancelledDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanCancelledDomainEventTests
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly LibraryScanCancelledDomainEventFixture _libraryScanCancelledDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        ScanId scanId = _scanIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create(id, scanId, libraryId, occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(scanId, domainEvent.ScanId);
        Assert.Equal(libraryId, domainEvent.LibraryId);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }
}
