#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ConsumptionTotalsDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionTotalsDtoMappingTests
{
    private readonly ConsumptionTotalsDtoFixture _consumptionTotalsDtoFixture = new();
    private readonly MediaKindConsumptionStatisticsDtoFixture _mediaKindConsumptionStatisticsDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingConsumptionTotals_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MediaKindConsumptionStatisticsDto mediaKindSummary = _mediaKindConsumptionStatisticsDtoFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            totalConsumedDurationMilliseconds: 1_500,
            countedSessionCount: 4,
            completedSessionCount: 3,
            stoppedSessionCount: 1,
            completionRate: 0.75);
        ConsumptionTotalsDto totals = _consumptionTotalsDtoFixture.Create(
            totalConsumedDurationMilliseconds: 2_500,
            countedSessionCount: 6,
            completedSessionCount: 4,
            stoppedSessionCount: 2,
            completionRate: 0.666,
            mediaKindSummaries: [mediaKindSummary]);

        // Act
        ConsumptionTotalsResponse result = totals.ToResponse();

        // Assert
        Assert.Equal(2.5, result.TotalConsumedDurationInSeconds);
        Assert.Equal(6, result.CountedSessionCount);
        Assert.Equal(4, result.CompletedSessionCount);
        Assert.Equal(2, result.StoppedSessionCount);
        Assert.Equal(0.666, result.CompletionRate);
        Assert.Single(result.MediaKindSummaries);
        Assert.Equal(MediaKind.MusicTrack, result.MediaKindSummaries[0].MediaKind);
        Assert.Equal(1.5, result.MediaKindSummaries[0].TotalConsumedDurationInSeconds);
        Assert.Equal(4, result.MediaKindSummaries[0].CountedSessionCount);
        Assert.Equal(3, result.MediaKindSummaries[0].CompletedSessionCount);
        Assert.Equal(1, result.MediaKindSummaries[0].StoppedSessionCount);
        Assert.Equal(0.75, result.MediaKindSummaries[0].CompletionRate);
    }

    [Fact]
    public void ToResponse_WhenMappingMultipleMediaKindSummaries_ShouldMapThemInOrder()
    {
        // Arrange
        MediaKindConsumptionStatisticsDto firstSummary = _mediaKindConsumptionStatisticsDtoFixture.Create(
            mediaKind: MediaKind.Book,
            totalConsumedDurationMilliseconds: 1_000,
            countedSessionCount: 2,
            completedSessionCount: 1,
            stoppedSessionCount: 1,
            completionRate: 0.5);
        MediaKindConsumptionStatisticsDto secondSummary = _mediaKindConsumptionStatisticsDtoFixture.Create(
            mediaKind: MediaKind.Movie,
            totalConsumedDurationMilliseconds: 4_000,
            countedSessionCount: 5,
            completedSessionCount: 4,
            stoppedSessionCount: 1,
            completionRate: 0.8);
        ConsumptionTotalsDto totals = _consumptionTotalsDtoFixture.Create(mediaKindSummaries: [firstSummary, secondSummary]);

        // Act
        ConsumptionTotalsResponse result = totals.ToResponse();

        // Assert
        Assert.Equal(2, result.MediaKindSummaries.Count);
        Assert.Equal(MediaKind.Book, result.MediaKindSummaries[0].MediaKind);
        Assert.Equal(1.0, result.MediaKindSummaries[0].TotalConsumedDurationInSeconds);
        Assert.Equal(2, result.MediaKindSummaries[0].CountedSessionCount);
        Assert.Equal(0.5, result.MediaKindSummaries[0].CompletionRate);
        Assert.Equal(MediaKind.Movie, result.MediaKindSummaries[1].MediaKind);
        Assert.Equal(4.0, result.MediaKindSummaries[1].TotalConsumedDurationInSeconds);
        Assert.Equal(5, result.MediaKindSummaries[1].CountedSessionCount);
        Assert.Equal(0.8, result.MediaKindSummaries[1].CompletionRate);
    }

    [Fact]
    public void ToResponse_WhenThereAreNoMediaKindSummaries_ShouldMapAnEmptyList()
    {
        // Arrange
        ConsumptionTotalsDto totals = _consumptionTotalsDtoFixture.Create(includeMediaKindSummaries: false);

        // Act
        ConsumptionTotalsResponse result = totals.ToResponse();

        // Assert
        Assert.NotNull(result.MediaKindSummaries);
        Assert.Empty(result.MediaKindSummaries);
    }

    [Fact]
    public void ToResponse_WhenDurationExceedsTimeSpanRange_ShouldClampToMaximumRepresentableSeconds()
    {
        // Arrange
        ConsumptionTotalsDto totals = _consumptionTotalsDtoFixture.Create(totalConsumedDurationMilliseconds: long.MaxValue);
        double expectedClampedSeconds = TimeSpan.FromMilliseconds(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond).TotalSeconds;

        // Act
        ConsumptionTotalsResponse result = totals.ToResponse();

        // Assert
        Assert.Equal(expectedClampedSeconds, result.TotalConsumedDurationInSeconds);
        Assert.True(result.TotalConsumedDurationInSeconds <= TimeSpan.MaxValue.TotalSeconds);
    }
}
