#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ConsumptionStatisticsDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionStatisticsDtoMappingTests
{
    private readonly ConsumptionStatisticsDtoFixture _consumptionStatisticsDtoFixture = new();
    private readonly MediaKindConsumptionStatisticsDtoFixture _mediaKindConsumptionStatisticsDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingConsumptionStatistics_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaKindConsumptionStatisticsDto mediaKindSummary = _mediaKindConsumptionStatisticsDtoFixture.Create(
            mediaKind: MediaKind.Book,
            totalConsumedDurationMilliseconds: 1_500,
            countedSessionCount: 8,
            completedSessionCount: 6,
            stoppedSessionCount: 2,
            completionRate: 0.75);
        ConsumptionStatisticsDto statistics = _consumptionStatisticsDtoFixture.Create(
            userId: userId,
            totalConsumedDurationMilliseconds: 3_000,
            countedSessionCount: 10,
            completedSessionCount: 7,
            stoppedSessionCount: 3,
            completionRate: 0.7,
            mediaKindSummaries: [mediaKindSummary]);

        // Act
        ConsumptionSummaryResponse result = statistics.ToResponse();

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal(3.0, result.TotalConsumedDurationInSeconds);
        Assert.Equal(10, result.CountedSessionCount);
        Assert.Equal(7, result.CompletedSessionCount);
        Assert.Equal(3, result.StoppedSessionCount);
        Assert.Equal(0.7, result.CompletionRate);
        Assert.Single(result.MediaKindSummaries);
        Assert.Equal(MediaKind.Book, result.MediaKindSummaries[0].MediaKind);
        Assert.Equal(1.5, result.MediaKindSummaries[0].TotalConsumedDurationInSeconds);
        Assert.Equal(8, result.MediaKindSummaries[0].CountedSessionCount);
        Assert.Equal(6, result.MediaKindSummaries[0].CompletedSessionCount);
        Assert.Equal(2, result.MediaKindSummaries[0].StoppedSessionCount);
        Assert.Equal(0.75, result.MediaKindSummaries[0].CompletionRate);
    }

    [Fact]
    public void ToResponse_WhenMappingMultipleMediaKindSummaries_ShouldMapThemInOrder()
    {
        // Arrange
        MediaKindConsumptionStatisticsDto firstSummary = _mediaKindConsumptionStatisticsDtoFixture.Create(
            mediaKind: MediaKind.Movie,
            totalConsumedDurationMilliseconds: 1_000,
            countedSessionCount: 3,
            completedSessionCount: 2,
            stoppedSessionCount: 1,
            completionRate: 0.666);
        MediaKindConsumptionStatisticsDto secondSummary = _mediaKindConsumptionStatisticsDtoFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            totalConsumedDurationMilliseconds: 2_500,
            countedSessionCount: 9,
            completedSessionCount: 8,
            stoppedSessionCount: 1,
            completionRate: 0.888);
        ConsumptionStatisticsDto statistics = _consumptionStatisticsDtoFixture.Create(
            mediaKindSummaries: [firstSummary, secondSummary]);

        // Act
        ConsumptionSummaryResponse result = statistics.ToResponse();

        // Assert
        Assert.Equal(2, result.MediaKindSummaries.Count);
        Assert.Equal(MediaKind.Movie, result.MediaKindSummaries[0].MediaKind);
        Assert.Equal(1.0, result.MediaKindSummaries[0].TotalConsumedDurationInSeconds);
        Assert.Equal(3, result.MediaKindSummaries[0].CountedSessionCount);
        Assert.Equal(2, result.MediaKindSummaries[0].CompletedSessionCount);
        Assert.Equal(1, result.MediaKindSummaries[0].StoppedSessionCount);
        Assert.Equal(0.666, result.MediaKindSummaries[0].CompletionRate);
        Assert.Equal(MediaKind.MusicTrack, result.MediaKindSummaries[1].MediaKind);
        Assert.Equal(2.5, result.MediaKindSummaries[1].TotalConsumedDurationInSeconds);
        Assert.Equal(9, result.MediaKindSummaries[1].CountedSessionCount);
        Assert.Equal(8, result.MediaKindSummaries[1].CompletedSessionCount);
        Assert.Equal(1, result.MediaKindSummaries[1].StoppedSessionCount);
        Assert.Equal(0.888, result.MediaKindSummaries[1].CompletionRate);
    }

    [Fact]
    public void ToResponse_WhenThereAreNoMediaKindSummaries_ShouldMapAnEmptyList()
    {
        // Arrange
        ConsumptionStatisticsDto statistics = _consumptionStatisticsDtoFixture.Create(includeMediaKindSummaries: false);

        // Act
        ConsumptionSummaryResponse result = statistics.ToResponse();

        // Assert
        Assert.Empty(result.MediaKindSummaries);
    }

    [Fact]
    public void ToResponse_WhenDurationExceedsTimeSpanRange_ShouldClampToMaximumRepresentableSeconds()
    {
        // Arrange
        ConsumptionStatisticsDto statistics = _consumptionStatisticsDtoFixture.Create(totalConsumedDurationMilliseconds: long.MaxValue);
        double expectedClampedSeconds = TimeSpan.FromMilliseconds(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond).TotalSeconds;

        // Act
        ConsumptionSummaryResponse result = statistics.ToResponse();

        // Assert
        Assert.Equal(expectedClampedSeconds, result.TotalConsumedDurationInSeconds);
        Assert.True(result.TotalConsumedDurationInSeconds <= TimeSpan.MaxValue.TotalSeconds);
    }
}
