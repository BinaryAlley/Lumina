#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanFailedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanFailedDomainEventTests
{
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    [Fact]
    public void Constructor_WhenCalledWithErrorMessage_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;
        string errorMessage = "Scan failed unexpectedly.";

        // Act
        LibraryScanFailedDomainEvent domainEvent = new(id, libraryId, compositeId, occurredOnUtc, errorMessage);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(libraryId, domainEvent.LibraryId);
        Assert.Equal(compositeId, domainEvent.MediaLibraryScanCompositeId);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.Equal(errorMessage, domainEvent.ErrorMessage);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }

    [Fact]
    public void Constructor_WhenCalledWithoutErrorMessage_ShouldDefaultErrorMessageToNull()
    {
        // Act
        LibraryScanFailedDomainEvent domainEvent = new(
            Guid.NewGuid(),
            _libraryIdFixture.Create(),
            _mediaLibraryScanCompositeIdFixture.Create(),
            DateTime.UtcNow);

        // Assert
        Assert.Null(domainEvent.ErrorMessage);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
