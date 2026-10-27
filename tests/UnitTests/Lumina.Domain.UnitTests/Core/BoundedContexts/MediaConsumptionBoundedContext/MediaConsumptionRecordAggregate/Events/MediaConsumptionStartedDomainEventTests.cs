#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionStartedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionStartedDomainEventTests
{
    private readonly MediaConsumptionRecordIdFixture _mediaConsumptionRecordIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly MediaConsumptionStartedDomainEventFixture _mediaConsumptionStartedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        MediaConsumptionRecordId mediaConsumptionRecordId = _mediaConsumptionRecordIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        MediaKind mediaKind = MediaKind.Movie;
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        MediaConsumptionStartedDomainEvent domainEvent = _mediaConsumptionStartedDomainEventFixture.Create(
            id,
            mediaConsumptionRecordId,
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(mediaConsumptionRecordId, domainEvent.MediaConsumptionRecordId);
        Assert.Equal(userId, domainEvent.UserId);
        Assert.Equal(libraryId, domainEvent.LibraryId);
        Assert.Equal(mediaItemId, domainEvent.MediaItemId);
        Assert.Equal(mediaKind, domainEvent.MediaKind);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
