#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="DailySchedule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DailyScheduleTests
{
    [Fact]
    public void Create_WhenCalledWithValidHourAndMinute_ShouldCreateDailyScheduleWithAllPropertiesSet()
    {
        // Act
        Result<DailySchedule> result = DailySchedule.Create(8, 30);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(8, result.Value.Hour);
        Assert.Equal(30, result.Value.Minute);
        Assert.Equal(ScheduleType.DailyAtHourAndMinute, result.Value.ScheduleType);
    }

    [Theory]
    [InlineData(-1)] // hour below the allowed range
    [InlineData(24)] // hour above the allowed range
    public void Create_WhenHourIsOutOfRange_ShouldReturnError(int hour)
    {
        // Act
        Result<DailySchedule> result = DailySchedule.Create(hour, 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.HourMustBeBetweenZeroAndTwentyThree, result.FirstError);
    }

    [Theory]
    [InlineData(-1)] // minute below the allowed range
    [InlineData(60)] // minute above the allowed range
    public void Create_WhenMinuteIsOutOfRange_ShouldReturnError(int minute)
    {
        // Act
        Result<DailySchedule> result = DailySchedule.Create(8, minute);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.MinuteMustBeBetweenZeroAndFiftyNine, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithLowerBoundaryValues_ShouldCreateDailySchedule()
    {
        // Act
        Result<DailySchedule> result = DailySchedule.Create(0, 0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0, result.Value.Hour);
        Assert.Equal(0, result.Value.Minute);
    }

    [Fact]
    public void Create_WhenCalledWithUpperBoundaryValues_ShouldCreateDailySchedule()
    {
        // Act
        Result<DailySchedule> result = DailySchedule.Create(23, 59);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(23, result.Value.Hour);
        Assert.Equal(59, result.Value.Minute);
    }

    [Fact]
    public void GetDelayUntilNextExecution_WhenNextRunIsLaterToday_ShouldReturnDelayUntilNextRun()
    {
        // Arrange
        DailySchedule schedule = DailySchedule.Create(12, 30).Value;
        DateTime utcNow = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        // Act
        TimeSpan delay = schedule.GetDelayUntilNextExecution(utcNow, TimeZoneInfo.Utc);

        // Assert
        Assert.Equal(TimeSpan.FromHours(2.5), delay);
    }

    [Fact]
    public void GetDelayUntilNextExecution_WhenNextRunAlreadyPassedToday_ShouldReturnDelayUntilTomorrow()
    {
        // Arrange
        DailySchedule schedule = DailySchedule.Create(8, 0).Value;
        DateTime utcNow = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        // Act
        TimeSpan delay = schedule.GetDelayUntilNextExecution(utcNow, TimeZoneInfo.Utc);

        // Assert
        Assert.Equal(TimeSpan.FromHours(22), delay);
    }

    [Fact]
    public void GetDelayUntilNextExecution_WhenNextRunEqualsNow_ShouldReturnDelayUntilTomorrow()
    {
        // Arrange
        DailySchedule schedule = DailySchedule.Create(10, 0).Value;
        DateTime utcNow = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        // Act
        TimeSpan delay = schedule.GetDelayUntilNextExecution(utcNow, TimeZoneInfo.Utc);

        // Assert
        Assert.Equal(TimeSpan.FromDays(1), delay);
    }

    [Fact]
    public void Equals_WhenHourAndMinuteAreEqual_ShouldReturnTrue()
    {
        // Act
        bool areEqual = DailySchedule.Create(8, 30).Value.Equals(DailySchedule.Create(8, 30).Value);

        // Assert
        Assert.True(areEqual);
    }

    [Theory]
    [InlineData(9, 30)] // different hour
    [InlineData(8, 31)] // different minute
    public void Equals_WhenHourOrMinuteDiffer_ShouldReturnFalse(int hour, int minute)
    {
        // Act
        bool areEqual = DailySchedule.Create(8, 30).Value.Equals(DailySchedule.Create(hour, minute).Value);

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void GetHashCode_WhenHourAndMinuteAreEqual_ShouldReturnEqualHashCodes()
    {
        // Act
        int firstHashCode = DailySchedule.Create(8, 30).Value.GetHashCode();
        int secondHashCode = DailySchedule.Create(8, 30).Value.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }
}
