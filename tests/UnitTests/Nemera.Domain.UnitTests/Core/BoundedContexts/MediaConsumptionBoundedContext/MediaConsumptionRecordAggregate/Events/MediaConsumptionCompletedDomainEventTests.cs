#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Events;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Nemera.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Nemera.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Nemera.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Nemera.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionCompletedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionCompletedDomainEventTests
{
    private readonly MediaConsumptionRecordIdFixture _mediaConsumptionRecordIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly MediaPositionFixture _mediaPositionFixture = new();
    private readonly MediaConsumptionCompletedDomainEventFixture _mediaConsumptionCompletedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        MediaConsumptionRecordId mediaConsumptionRecordId = _mediaConsumptionRecordIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);
        double lastProgressFraction = 1.0;
        TimeSpan consumedDuration = TimeSpan.FromHours(2);
        int consecutivePagesRead = 12;
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        MediaConsumptionCompletedDomainEvent domainEvent = _mediaConsumptionCompletedDomainEventFixture.Create(
            id,
            mediaConsumptionRecordId,
            userId,
            libraryId,
            mediaItemId,
            MediaKind.Book,
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
        Assert.Equal(MediaKind.Book, domainEvent.MediaKind);
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
        MediaConsumptionCompletedDomainEvent domainEvent = _mediaConsumptionCompletedDomainEventFixture.Create(mediaKind: MediaKind.Photo);

        // Act
        Optional<MediaPosition> lastPosition = domainEvent.LastPosition;

        // Assert
        Assert.False(lastPosition.HasValue);
    }
}
