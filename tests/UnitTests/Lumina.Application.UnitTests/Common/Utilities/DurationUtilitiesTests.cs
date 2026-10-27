#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="DurationUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DurationUtilitiesTests
{
    [Fact]
    public void ToClampedTimeSpan_WhenMillisecondsIsZero_ShouldReturnTimeSpanZero()
    {
        // Act
        TimeSpan result = 0L.ToClampedTimeSpan();

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    [Theory]
    [InlineData(1, 0.001)] // a single millisecond is preserved
    [InlineData(1_500, 1.5)] // whole milliseconds convert to fractional seconds
    [InlineData(60_000, 60)] // a full minute converts exactly
    public void ToClampedTimeSpan_WhenMillisecondsIsWithinTheRepresentableRange_ShouldConvertToTheEquivalentSeconds(long milliseconds, double expectedTotalSeconds)
    {
        // Act
        TimeSpan result = milliseconds.ToClampedTimeSpan();

        // Assert
        Assert.Equal(expectedTotalSeconds, result.TotalSeconds);
    }

    [Theory]
    [InlineData(-1)] // a single negative millisecond
    [InlineData(-1_500)] // negative fractional seconds
    public void ToClampedTimeSpan_WhenMillisecondsIsNegative_ShouldReturnTheEquivalentNegativeDuration(long milliseconds)
    {
        // Act
        TimeSpan result = milliseconds.ToClampedTimeSpan();

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(milliseconds), result);
        Assert.True(result < TimeSpan.Zero);
    }

    [Fact]
    public void ToClampedTimeSpan_WhenMillisecondsIsTheMaximumRepresentableValue_ShouldReturnTheEquivalentDuration()
    {
        // Arrange
        long maximumMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;

        // Act
        TimeSpan result = maximumMilliseconds.ToClampedTimeSpan();

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(maximumMilliseconds), result);
        Assert.True(result <= TimeSpan.MaxValue);
    }

    [Fact]
    public void ToClampedTimeSpan_WhenMillisecondsIsLongMaxValue_ShouldClampToTheMaximumRepresentableDuration()
    {
        // Arrange
        long maximumMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
        TimeSpan expectedClampedDuration = TimeSpan.FromMilliseconds(maximumMilliseconds);

        // Act
        TimeSpan result = long.MaxValue.ToClampedTimeSpan();

        // Assert
        Assert.Equal(expectedClampedDuration, result);
        Assert.Equal(expectedClampedDuration.TotalSeconds, result.TotalSeconds);
        Assert.True(double.IsFinite(result.TotalSeconds));
        Assert.True(result <= TimeSpan.MaxValue);
    }

    [Fact]
    public void ToClampedTimeSpan_WhenMillisecondsIsOneAboveTheMaximumRepresentableValue_ShouldClampToTheMaximumRepresentableDuration()
    {
        // Arrange
        long maximumMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
        TimeSpan expectedClampedDuration = TimeSpan.FromMilliseconds(maximumMilliseconds);

        // Act
        TimeSpan result = (maximumMilliseconds + 1).ToClampedTimeSpan();

        // Assert
        Assert.Equal(expectedClampedDuration, result);
        Assert.True(result <= TimeSpan.MaxValue);
    }
}
