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
/// Contains unit tests for the <see cref="MediaKindConsumptionStatisticsDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaKindConsumptionStatisticsDtoMappingTests
{
    private readonly MediaKindConsumptionStatisticsDtoFixture _mediaKindConsumptionStatisticsDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingMediaKindStatistics_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MediaKindConsumptionStatisticsDto statistics = _mediaKindConsumptionStatisticsDtoFixture.Create(
            mediaKind: MediaKind.Audiobook,
            totalConsumedDurationMilliseconds: 4_500,
            countedSessionCount: 12,
            completedSessionCount: 9,
            stoppedSessionCount: 3,
            completionRate: 0.75);

        // Act
        MediaKindConsumptionSummaryResponse result = statistics.ToResponse();

        // Assert
        Assert.Equal(MediaKind.Audiobook, result.MediaKind);
        Assert.Equal(4.5, result.TotalConsumedDurationInSeconds);
        Assert.Equal(12, result.CountedSessionCount);
        Assert.Equal(9, result.CompletedSessionCount);
        Assert.Equal(3, result.StoppedSessionCount);
        Assert.Equal(0.75, result.CompletionRate);
    }

    [Theory]
    [InlineData(1_500, 1.5)] // half a second survives the conversion
    [InlineData(2_000, 2)] // a whole number of seconds stays whole
    public void ToResponse_WhenDurationIsInMilliseconds_ShouldConvertToSeconds(long consumedDurationMilliseconds, double expectedSeconds)
    {
        // Arrange
        MediaKindConsumptionStatisticsDto statistics = _mediaKindConsumptionStatisticsDtoFixture.Create(
            totalConsumedDurationMilliseconds: consumedDurationMilliseconds);

        // Act
        MediaKindConsumptionSummaryResponse result = statistics.ToResponse();

        // Assert
        Assert.Equal(expectedSeconds, result.TotalConsumedDurationInSeconds);
    }

    [Fact]
    public void ToResponse_WhenDurationExceedsTimeSpanRange_ShouldClampToMaximumRepresentableSeconds()
    {
        // Arrange
        MediaKindConsumptionStatisticsDto statistics = _mediaKindConsumptionStatisticsDtoFixture.Create(
            totalConsumedDurationMilliseconds: long.MaxValue);
        double expectedClampedSeconds = TimeSpan.FromMilliseconds(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond).TotalSeconds;

        // Act
        MediaKindConsumptionSummaryResponse result = statistics.ToResponse();

        // Assert
        Assert.Equal(expectedClampedSeconds, result.TotalConsumedDurationInSeconds);
        Assert.True(result.TotalConsumedDurationInSeconds <= TimeSpan.MaxValue.TotalSeconds);
    }
}
