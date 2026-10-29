#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionProgressedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionProgressedDomainEventTests
{
    private readonly MediaConsumptionRecordIdFixture _mediaConsumptionRecordIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly MediaPositionFixture _mediaPositionFixture = new();
    private readonly MediaConsumptionProgressedDomainEventFixture _mediaConsumptionProgressedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        MediaConsumptionRecordId mediaConsumptionRecordId = _mediaConsumptionRecordIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        double lastProgressFraction = 0.4;
        TimeSpan consumedDuration = TimeSpan.FromMinutes(25);
        int consecutivePagesRead = 0;
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        MediaConsumptionProgressedDomainEvent domainEvent = _mediaConsumptionProgressedDomainEventFixture.Create(
            id,
            mediaConsumptionRecordId,
            userId,
            libraryId,
            mediaItemId,
            MediaKind.Movie,
            lastPosition: Optional<MediaPosition>.Some(lastPosition),
            lastProgressFraction: lastProgressFraction,
            consumedDuration: consumedDuration,
            consecutivePagesRead: consecutivePagesRead,
            occurredOnUtc: occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(mediaConsumptionRecordId, domainEvent.MediaConsumptionRecordId);
        Assert.Equal(userId, domainEvent.UserId);
        Assert.Equal(libraryId, domainEvent.LibraryId);
        Assert.Equal(mediaItemId, domainEvent.MediaItemId);
        Assert.Equal(MediaKind.Movie, domainEvent.MediaKind);
        Assert.Equal(Optional<MediaPosition>.Some(lastPosition), domainEvent.LastPosition);
        Assert.Equal(lastProgressFraction, domainEvent.LastProgressFraction);
        Assert.Equal(consumedDuration, domainEvent.ConsumedDuration);
        Assert.Equal(consecutivePagesRead, domainEvent.ConsecutivePagesRead);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }

    [Fact]
    public void Constructor_WhenLastPositionIsAbsent_ShouldSetNoLastPosition()
    {
        // Arrange
        MediaConsumptionProgressedDomainEvent domainEvent = _mediaConsumptionProgressedDomainEventFixture.Create(mediaKind: MediaKind.Movie);

        // Act
        Optional<MediaPosition> lastPosition = domainEvent.LastPosition;

        // Assert
        Assert.False(lastPosition.HasValue);
    }
}
