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
/// Contains unit tests for the <see cref="IntervalSchedule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IntervalScheduleTests
{
    [Fact]
    public void Create_WhenCalledWithValidInterval_ShouldCreateIntervalScheduleWithAllPropertiesSet()
    {
        // Act
        Result<IntervalSchedule> result = IntervalSchedule.Create(60);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(60, result.Value.IntervalMinutes);
        Assert.Equal(ScheduleType.WithIntervalInMinutes, result.Value.ScheduleType);
    }

    [Theory]
    [InlineData(0)] // interval of zero
    [InlineData(-1)] // negative interval
    [InlineData(-100)] // negative interval far from zero
    public void Create_WhenIntervalIsNotPositive_ShouldReturnError(int intervalMinutes)
    {
        // Act
        Result<IntervalSchedule> result = IntervalSchedule.Create(intervalMinutes);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.IntervalMinutesMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithOneMinuteInterval_ShouldCreateIntervalSchedule()
    {
        // Act
        Result<IntervalSchedule> result = IntervalSchedule.Create(1);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1, result.Value.IntervalMinutes);
    }

    [Fact]
    public void GetDelayUntilNextExecution_WhenCalled_ShouldReturnTheIntervalInMinutes()
    {
        // Arrange
        IntervalSchedule schedule = IntervalSchedule.Create(45).Value;
        DateTime utcNow = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        // Act
        TimeSpan delay = schedule.GetDelayUntilNextExecution(utcNow, TimeZoneInfo.Utc);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(45), delay);
    }

    [Fact]
    public void Equals_WhenIntervalsAreEqual_ShouldReturnTrue()
    {
        // Act
        bool areEqual = IntervalSchedule.Create(60).Value.Equals(IntervalSchedule.Create(60).Value);

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void Equals_WhenIntervalsDiffer_ShouldReturnFalse()
    {
        // Act
        bool areEqual = IntervalSchedule.Create(60).Value.Equals(IntervalSchedule.Create(120).Value);

        // Assert
        Assert.False(areEqual);
    }
}
