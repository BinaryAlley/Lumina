#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionRecordEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordEntityMappingTests
{
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenEntityIsValid_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime startedOnUtc = new(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2024, 3, 1, 10, 30, 0, DateTimeKind.Utc);
        DateTime endedOnUtc = new(2024, 3, 1, 10, 40, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            id: id,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: endedOnUtc,
            consumedDurationMilliseconds: 1_500,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 2_500,
            lastProgressFraction: 0.75,
            consecutivePagesRead: 0);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        MediaConsumptionRecord record = result.Value;
        Assert.Equal(id, record.Id.Value);
        Assert.Equal(userId, record.UserId.Value);
        Assert.Equal(libraryId, record.LibraryId.Value);
        Assert.Equal(mediaItemId, record.MediaItemId.Value);
        Assert.Equal(MediaKind.Movie, record.MediaKind);
        Assert.Equal(ConsumptionStatus.Completed, record.Status);
        Assert.Equal(startedOnUtc, record.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, record.LastActivityOnUtc);
        Assert.True(record.EndedOnUtc.HasValue);
        Assert.Equal(endedOnUtc, record.EndedOnUtc.Value);
        Assert.Equal(TimeSpan.FromMilliseconds(1_500), record.ConsumedDuration);
        Assert.True(record.LastPosition.HasValue);
        Assert.Equal(MediaPositionKind.TimeOffset, record.LastPosition.Value.Kind);
        Assert.True(record.LastPosition.Value.TimeOffset.HasValue);
        Assert.Equal(TimeSpan.FromMilliseconds(2_500), record.LastPosition.Value.TimeOffset.Value);
        Assert.False(record.LastPosition.Value.Location.HasValue);
        Assert.Equal(0.75, record.LastProgressFraction);
        Assert.Equal(0, record.ConsecutivePagesRead);
        Assert.Equal(startedOnUtc, record.CreatedOnUtc);
    }

    [Fact]
    public void ToDomainEntity_WhenEntityHasNoPosition_ShouldMapRecordWithoutAPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            includeLastPositionKind: false,
            includeLastPositionTimeOffsetMilliseconds: false,
            includeLastPositionLocation: false);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.LastPosition.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenPositionIsTimeOffset_ShouldMapTheSameTimeSpan()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 1_234);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.LastPosition.HasValue);
        Assert.Equal(MediaPositionKind.TimeOffset, result.Value.LastPosition.Value.Kind);
        Assert.Equal(TimeSpan.FromMilliseconds(1_234), result.Value.LastPosition.Value.TimeOffset.Value);
        Assert.False(result.Value.LastPosition.Value.Location.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenPositionIsLocation_ShouldMapTheSameLocation()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Book,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionLocation: "chapter-1",
            consecutivePagesRead: 5);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.LastPosition.HasValue);
        Assert.Equal(MediaPositionKind.Location, result.Value.LastPosition.Value.Kind);
        Assert.False(result.Value.LastPosition.Value.TimeOffset.HasValue);
        Assert.Equal("chapter-1", result.Value.LastPosition.Value.Location.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenEntityIsInProgress_ShouldMapNoEndTime()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            includeEndedOnUtc: false);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.EndedOnUtc.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenEntityIsTerminal_ShouldMapTheEndTime()
    {
        // Arrange
        DateTime lastActivityOnUtc = new(2024, 3, 1, 10, 30, 0, DateTimeKind.Utc);
        DateTime endedOnUtc = new(2024, 3, 1, 10, 45, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.Stopped,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: endedOnUtc);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.EndedOnUtc.HasValue);
        Assert.Equal(endedOnUtc, result.Value.EndedOnUtc.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenTimestampsAreUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime startedOnUtc = new(2024, 4, 2, 6, 15, 0, DateTimeKind.Unspecified);
        DateTime lastActivityOnUtc = new(2024, 4, 2, 6, 45, 0, DateTimeKind.Unspecified);
        DateTime endedOnUtc = new(2024, 4, 2, 7, 0, 0, DateTimeKind.Unspecified);
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: endedOnUtc);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeKind.Utc, result.Value.StartedOnUtc.Kind);
        Assert.Equal(startedOnUtc.Ticks, result.Value.StartedOnUtc.Ticks);
        Assert.Equal(DateTimeKind.Utc, result.Value.LastActivityOnUtc.Kind);
        Assert.Equal(lastActivityOnUtc.Ticks, result.Value.LastActivityOnUtc.Ticks);
        Assert.True(result.Value.EndedOnUtc.HasValue);
        Assert.Equal(DateTimeKind.Utc, result.Value.EndedOnUtc.Value.Kind);
        Assert.Equal(endedOnUtc.Ticks, result.Value.EndedOnUtc.Value.Ticks);
    }

    [Fact]
    public void ToDomainEntity_WhenDurationExceedsTimeSpanRange_ShouldClampToMaximumTimeSpan()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            consumedDurationMilliseconds: long.MaxValue);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TimeSpan.FromMilliseconds(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond), result.Value.ConsumedDuration);
        Assert.True(result.Value.ConsumedDuration <= TimeSpan.MaxValue);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionHasNoOffset_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.TimeOffset,
            includeLastPositionTimeOffsetMilliseconds: false,
            includeLastPositionLocation: false);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionIsNegative_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: -1);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionIsAboveTheRepresentableRange_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        long maximumTimeOffsetMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: maximumTimeOffsetMilliseconds + 1);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionAlsoHasALocation_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionLocation: "chapter-1");

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenLocationPositionHasNoLocation_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionLocation: false,
            includeLastPositionTimeOffsetMilliseconds: false);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Theory]
    [InlineData("")] // an empty location is not a usable position
    [InlineData("   ")] // a whitespace only location is not a usable position
    public void ToDomainEntity_WhenLocationPositionHasABlankLocation_ShouldReturnInvalidMediaPosition(string location)
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionLocation: location);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenLocationPositionAlsoHasATimeOffset_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionTimeOffsetMilliseconds: 1_000,
            lastPositionLocation: "chapter-1");

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenPositionKindIsUnsupported_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: (MediaPositionKind)int.MaxValue);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenPositionKindDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionLocation: "chapter-1");

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenConsecutivePagesArePositiveForANonPageBasedKind_ShouldReturnTheDomainError()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consecutivePagesRead: 5);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenProgressFractionIsOutsideItsBounds_ShouldReturnTheDomainError()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            lastProgressFraction: 1.5);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenAnInProgressEntityHasAnEndTime_ShouldReturnTheDomainError()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            endedOnUtc: DateTime.UtcNow);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InProgressConsumptionCannotHaveEndTime, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenATerminalEntityHasNoEndTime_ShouldReturnTheDomainError()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.Completed,
            includeEndedOnUtc: false);

        // Act
        Result<MediaConsumptionRecord> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.EndedConsumptionMustHaveEndTime, result.FirstError);
    }

    [Fact]
    public void ToDto_WhenMappingEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime startedOnUtc = new(2024, 8, 9, 12, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2024, 8, 9, 12, 20, 0, DateTimeKind.Utc);
        DateTime endedOnUtc = new(2024, 8, 9, 12, 25, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            id: id,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: endedOnUtc,
            consumedDurationMilliseconds: 1_234,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 2_500,
            lastProgressFraction: 0.75,
            consecutivePagesRead: 0);

        // Act
        ConsumptionRecordDto result = entity.ToDto();

        // Assert
        Assert.Equal(id, result.Id);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(mediaItemId, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(ConsumptionStatus.Completed, result.Status);
        Assert.Equal(startedOnUtc, result.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, result.LastActivityOnUtc);
        Assert.Equal(endedOnUtc, result.EndedOnUtc);
        Assert.Equal(1_234L, result.ConsumedDurationMilliseconds);
        Assert.NotNull(result.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(2_500L, result.LastPosition.TimeOffsetMilliseconds);
        Assert.Null(result.LastPosition.Location);
        Assert.Equal(0.75, result.LastProgressFraction);
        Assert.Equal(0, result.ConsecutivePagesRead);
    }

    [Fact]
    public void ToDto_WhenEntityHasNoPosition_ShouldMapNullPosition()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(includeLastPositionKind: false);

        // Act
        ConsumptionRecordDto result = entity.ToDto();

        // Assert
        Assert.Null(result.LastPosition);
    }

    [Fact]
    public void ToDto_WhenEndTimeIsMissing_ShouldMapNullEndTime()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            status: ConsumptionStatus.InProgress,
            includeEndedOnUtc: false);

        // Act
        ConsumptionRecordDto result = entity.ToDto();

        // Assert
        Assert.Null(result.EndedOnUtc);
    }

    [Fact]
    public void ToDto_WhenTimestampsAreUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime startedOnUtc = new(2024, 9, 10, 14, 5, 0, DateTimeKind.Unspecified);
        DateTime lastActivityOnUtc = new(2024, 9, 10, 14, 25, 0, DateTimeKind.Unspecified);
        DateTime endedOnUtc = new(2024, 9, 10, 14, 30, 0, DateTimeKind.Unspecified);
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: endedOnUtc);

        // Act
        ConsumptionRecordDto result = entity.ToDto();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.StartedOnUtc.Kind);
        Assert.Equal(startedOnUtc.Ticks, result.StartedOnUtc.Ticks);
        Assert.Equal(DateTimeKind.Utc, result.LastActivityOnUtc.Kind);
        Assert.Equal(lastActivityOnUtc.Ticks, result.LastActivityOnUtc.Ticks);
        Assert.NotNull(result.EndedOnUtc);
        Assert.Equal(DateTimeKind.Utc, result.EndedOnUtc.Value.Kind);
        Assert.Equal(endedOnUtc.Ticks, result.EndedOnUtc.Value.Ticks);
    }

    [Fact]
    public void ToDto_WhenDurationExceedsTimeSpanRange_ShouldPreserveTheRawMilliseconds()
    {
        // Arrange
        MediaConsumptionRecordEntity entity = _mediaConsumptionRecordEntityFixture.Create(
            consumedDurationMilliseconds: long.MaxValue);

        // Act
        ConsumptionRecordDto result = entity.ToDto();

        // Assert
        Assert.Equal(long.MaxValue, result.ConsumedDurationMilliseconds);
    }
}
