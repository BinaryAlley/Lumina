#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionRecordMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordMappingTests
{
    private readonly MediaConsumptionRecordFixture _mediaConsumptionRecordFixture = new();
    private readonly MediaPositionFixture _mediaPositionFixture = new();
    private readonly MediaConsumptionRecordIdFixture _mediaConsumptionRecordIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteRecord_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MediaConsumptionRecordId id = _mediaConsumptionRecordIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime startedOnUtc = new(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2024, 3, 1, 10, 30, 0, DateTimeKind.Utc);
        DateTime endedOnUtc = new(2024, 3, 1, 10, 40, 0, DateTimeKind.Utc);
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromMilliseconds(2_500));
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            id: id,
            userId: userId,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: Optional<DateTime>.Some(endedOnUtc),
            consumedDuration: TimeSpan.FromMilliseconds(1_500),
            lastPosition: Optional<MediaPosition>.Some(position),
            lastProgressFraction: 0.75,
            consecutivePagesRead: 0);

        // Act
        MediaConsumptionRecordEntity result = record.ToRepositoryEntity();

        // Assert
        Assert.Equal(id.Value, result.Id);
        Assert.Equal(userId.Value, result.UserId);
        Assert.Equal(libraryId.Value, result.LibraryId);
        Assert.Equal(mediaItemId.Value, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(ConsumptionStatus.Completed, result.Status);
        Assert.Equal(startedOnUtc, result.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, result.LastActivityOnUtc);
        Assert.Equal(endedOnUtc, result.EndedOnUtc);
        Assert.Equal(1_500L, result.ConsumedDurationMilliseconds);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPositionKind);
        Assert.Equal(2_500L, result.LastPositionTimeOffsetMilliseconds);
        Assert.Null(result.LastPositionLocation);
        Assert.Equal(0.75, result.LastProgressFraction);
        Assert.Equal(0, result.ConsecutivePagesRead);
        Assert.Equal(1L, result.Version);
        Assert.Equal(startedOnUtc, result.CreatedOnUtc);
        Assert.Equal(Guid.Empty, result.CreatedBy);
        Assert.Null(result.UpdatedOnUtc);
        Assert.Null(result.UpdatedBy);
    }

    [Fact]
    public void ToRepositoryEntity_WhenPositionIsTimeOffset_ShouldMapKindAndTruncatedMilliseconds()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromTicks(15_600));
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPosition: Optional<MediaPosition>.Some(position));

        // Act
        MediaConsumptionRecordEntity result = record.ToRepositoryEntity();

        // Assert
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPositionKind);
        Assert.Equal(1L, result.LastPositionTimeOffsetMilliseconds);
        Assert.Null(result.LastPositionLocation);
    }

    [Fact]
    public void ToRepositoryEntity_WhenPositionIsLocation_ShouldMapKindAndLocation()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1");
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            lastPosition: Optional<MediaPosition>.Some(position));

        // Act
        MediaConsumptionRecordEntity result = record.ToRepositoryEntity();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.LastPositionKind);
        Assert.Null(result.LastPositionTimeOffsetMilliseconds);
        Assert.Equal("chapter-1", result.LastPositionLocation);
    }

    [Fact]
    public void ToRepositoryEntity_WhenRecordHasNoPosition_ShouldMapAllPositionFieldsToNull()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            includeLastPosition: false);

        // Act
        MediaConsumptionRecordEntity result = record.ToRepositoryEntity();

        // Assert
        Assert.Null(result.LastPositionKind);
        Assert.Null(result.LastPositionTimeOffsetMilliseconds);
        Assert.Null(result.LastPositionLocation);
    }

    [Fact]
    public void ToRepositoryEntity_WhenDurationHasFractionalMilliseconds_ShouldTruncateTowardZero()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            consumedDuration: TimeSpan.FromTicks(15_600));

        // Act
        MediaConsumptionRecordEntity result = record.ToRepositoryEntity();

        // Assert
        Assert.Equal(1L, result.ConsumedDurationMilliseconds);
    }

    [Fact]
    public void ToDto_WhenMappingCompleteRecord_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MediaConsumptionRecordId id = _mediaConsumptionRecordIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime startedOnUtc = new(2024, 4, 1, 8, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2024, 4, 1, 8, 25, 0, DateTimeKind.Utc);
        DateTime endedOnUtc = new(2024, 4, 1, 8, 30, 0, DateTimeKind.Utc);
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromMilliseconds(2_500));
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            id: id,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: Optional<DateTime>.Some(endedOnUtc),
            consumedDuration: TimeSpan.FromMilliseconds(1_500),
            lastPosition: Optional<MediaPosition>.Some(position),
            lastProgressFraction: 0.75,
            consecutivePagesRead: 0);

        // Act
        ConsumptionRecordDto result = record.ToDto();

        // Assert
        Assert.Equal(id.Value, result.Id);
        Assert.Equal(libraryId.Value, result.LibraryId);
        Assert.Equal(mediaItemId.Value, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(ConsumptionStatus.Completed, result.Status);
        Assert.Equal(startedOnUtc, result.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, result.LastActivityOnUtc);
        Assert.Equal(endedOnUtc, result.EndedOnUtc);
        Assert.Equal(1_500L, result.ConsumedDurationMilliseconds);
        Assert.NotNull(result.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(2_500L, result.LastPosition.TimeOffsetMilliseconds);
        Assert.Null(result.LastPosition.Location);
        Assert.Equal(0.75, result.LastProgressFraction);
        Assert.Equal(0, result.ConsecutivePagesRead);
    }

    [Fact]
    public void ToDto_WhenRecordHasNoPosition_ShouldMapNullPosition()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(includeLastPosition: false);

        // Act
        ConsumptionRecordDto result = record.ToDto();

        // Assert
        Assert.Null(result.LastPosition);
    }

    [Fact]
    public void ToDto_WhenRecordHasNoEndTime_ShouldMapNullEndTime()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(status: ConsumptionStatus.InProgress);

        // Act
        ConsumptionRecordDto result = record.ToDto();

        // Assert
        Assert.Null(result.EndedOnUtc);
    }

    [Fact]
    public void ToDto_WhenTimestampsAreUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime startedOnUtc = new(2024, 4, 2, 6, 15, 0, DateTimeKind.Unspecified);
        DateTime lastActivityOnUtc = new(2024, 4, 2, 6, 45, 0, DateTimeKind.Unspecified);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc);

        // Act
        ConsumptionRecordDto result = record.ToDto();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.StartedOnUtc.Kind);
        Assert.Equal(startedOnUtc.Ticks, result.StartedOnUtc.Ticks);
        Assert.Equal(DateTimeKind.Utc, result.LastActivityOnUtc.Kind);
        Assert.Equal(lastActivityOnUtc.Ticks, result.LastActivityOnUtc.Ticks);
    }

    [Fact]
    public void ToDto_WhenDurationHasFractionalMilliseconds_ShouldTruncateTowardZero()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            consumedDuration: TimeSpan.FromTicks(15_600));

        // Act
        ConsumptionRecordDto result = record.ToDto();

        // Assert
        Assert.Equal(1L, result.ConsumedDurationMilliseconds);
    }

    [Fact]
    public void ToResponse_WhenMappingCompleteRecord_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MediaConsumptionRecordId id = _mediaConsumptionRecordIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime startedOnUtc = new(2024, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2024, 5, 1, 12, 20, 0, DateTimeKind.Utc);
        DateTime endedOnUtc = new(2024, 5, 1, 12, 25, 0, DateTimeKind.Utc);
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromMilliseconds(2_500));
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            id: id,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: Optional<DateTime>.Some(endedOnUtc),
            consumedDuration: TimeSpan.FromMilliseconds(1_500),
            lastPosition: Optional<MediaPosition>.Some(position),
            lastProgressFraction: 0.75,
            consecutivePagesRead: 0);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.Equal(id.Value, result.Id);
        Assert.Equal(libraryId.Value, result.LibraryId);
        Assert.Equal(mediaItemId.Value, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(ConsumptionStatus.Completed, result.Status);
        Assert.Equal(startedOnUtc, result.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, result.LastActivityOnUtc);
        Assert.Equal(endedOnUtc, result.EndedOnUtc);
        Assert.Equal(1.5, result.ConsumedDurationInSeconds);
        Assert.NotNull(result.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(2.5, result.LastPosition.TimeOffsetSeconds);
        Assert.Null(result.LastPosition.Location);
        Assert.Equal(0.75, result.LastProgressFraction);
        Assert.Equal(0, result.ConsecutivePagesRead);
    }

    [Fact]
    public void ToResponse_WhenRecordHasNoPosition_ShouldMapNullPosition()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(includeLastPosition: false);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.Null(result.LastPosition);
    }

    [Fact]
    public void ToResponse_WhenRecordHasNoEndTime_ShouldMapNullEndTime()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(status: ConsumptionStatus.InProgress);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.Null(result.EndedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenTimestampsAreUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime startedOnUtc = new(2024, 5, 2, 6, 15, 0, DateTimeKind.Unspecified);
        DateTime lastActivityOnUtc = new(2024, 5, 2, 6, 45, 0, DateTimeKind.Unspecified);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.StartedOnUtc.Kind);
        Assert.Equal(startedOnUtc.Ticks, result.StartedOnUtc.Ticks);
        Assert.Equal(DateTimeKind.Utc, result.LastActivityOnUtc.Kind);
        Assert.Equal(lastActivityOnUtc.Ticks, result.LastActivityOnUtc.Ticks);
    }

    [Fact]
    public void ToResponse_WhenDurationHasFractionalMilliseconds_ShouldKeepTheFractionalSeconds()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            consumedDuration: TimeSpan.FromTicks(15_600));

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.Equal(0.00156, result.ConsumedDurationInSeconds);
    }
}
