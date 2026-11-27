#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ConsumptionRecordDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionRecordDtoMappingTests
{
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly MediaPositionDtoFixture _mediaPositionDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingConsumptionRecord_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime startedOnUtc = new(2024, 6, 1, 18, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2024, 6, 1, 18, 42, 0, DateTimeKind.Utc);
        DateTime endedOnUtc = new(2024, 6, 1, 18, 45, 0, DateTimeKind.Utc);
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetMilliseconds: 2_500,
            includeLocation: false);
        ConsumptionRecordDto record = _consumptionRecordDtoFixture.Create(
            id: id,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: endedOnUtc,
            consumedDurationMilliseconds: 2_700,
            lastPosition: lastPosition,
            lastProgressFraction: 0.75,
            consecutivePagesRead: 0);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.Equal(id, result.Id);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(mediaItemId, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(ConsumptionStatus.Completed, result.Status);
        Assert.Equal(startedOnUtc, result.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, result.LastActivityOnUtc);
        Assert.Equal(endedOnUtc, result.EndedOnUtc);
        Assert.Equal(2.7, result.ConsumedDurationInSeconds);
        Assert.NotNull(result.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(2.5, result.LastPosition.TimeOffsetSeconds);
        Assert.Null(result.LastPosition.Location);
        Assert.Equal(0.75, result.LastProgressFraction);
        Assert.Equal(0, result.ConsecutivePagesRead);
    }

    [Fact]
    public void ToResponse_WhenEndTimeAndPositionAreMissing_ShouldMapNulls()
    {
        // Arrange
        ConsumptionRecordDto record = _consumptionRecordDtoFixture.Create(
            status: ConsumptionStatus.InProgress,
            includeEndedOnUtc: false,
            includeLastPosition: false);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.Null(result.EndedOnUtc);
        Assert.Null(result.LastPosition);
    }

    [Fact]
    public void ToResponse_WhenTimestampsAreUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime startedOnUtc = new(2024, 1, 15, 9, 30, 0, DateTimeKind.Unspecified);
        DateTime lastActivityOnUtc = new(2024, 1, 15, 9, 45, 0, DateTimeKind.Unspecified);
        DateTime endedOnUtc = new(2024, 1, 15, 9, 50, 0, DateTimeKind.Unspecified);
        ConsumptionRecordDto record = _consumptionRecordDtoFixture.Create(
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: endedOnUtc);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

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
    public void ToResponse_WhenDurationExceedsTimeSpanRange_ShouldClampToMaximumRepresentableSeconds()
    {
        // Arrange
        ConsumptionRecordDto record = _consumptionRecordDtoFixture.Create(consumedDurationMilliseconds: long.MaxValue);
        double expectedClampedSeconds = TimeSpan.FromMilliseconds(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond).TotalSeconds;

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.Equal(expectedClampedSeconds, result.ConsumedDurationInSeconds);
        Assert.True(result.ConsumedDurationInSeconds <= TimeSpan.MaxValue.TotalSeconds);
    }

    [Fact]
    public void ToResponse_WhenPositionIsTimeOffset_ShouldMapTheTimeOffsetResponse()
    {
        // Arrange
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetMilliseconds: 1_234,
            includeLocation: false);
        ConsumptionRecordDto record = _consumptionRecordDtoFixture.Create(lastPosition: lastPosition);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.NotNull(result.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(1.234, result.LastPosition.TimeOffsetSeconds);
        Assert.Null(result.LastPosition.Location);
    }

    [Fact]
    public void ToResponse_WhenPositionIsLocation_ShouldMapTheLocationResponse()
    {
        // Arrange
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1",
            includeTimeOffsetMilliseconds: false);
        ConsumptionRecordDto record = _consumptionRecordDtoFixture.Create(
            mediaKind: MediaKind.Book,
            lastPosition: lastPosition);

        // Act
        ConsumptionRecordResponse result = record.ToResponse();

        // Assert
        Assert.NotNull(result.LastPosition);
        Assert.Equal(MediaPositionKind.Location, result.LastPosition.Kind);
        Assert.Null(result.LastPosition.TimeOffsetSeconds);
        Assert.Equal("chapter-1", result.LastPosition.Location);
    }
}
