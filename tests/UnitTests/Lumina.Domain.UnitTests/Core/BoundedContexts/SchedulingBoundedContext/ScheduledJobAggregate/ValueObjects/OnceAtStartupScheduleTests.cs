#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="OnceAtStartupSchedule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OnceAtStartupScheduleTests
{
    private readonly OnceAtStartupScheduleFixture _onceAtStartupScheduleFixture = new();

    [Fact]
    public void Create_WhenCalled_ShouldCreateOnceAtStartupSchedule()
    {
        // Act
        OnceAtStartupSchedule schedule = _onceAtStartupScheduleFixture.Create();

        // Assert
        Assert.Equal(ScheduleType.OnceAtStartup, schedule.ScheduleType);
    }

    [Fact]
    public void GetDelayUntilNextExecution_WhenCalled_ShouldReturnTheMaximumDelay()
    {
        // Arrange
        OnceAtStartupSchedule schedule = _onceAtStartupScheduleFixture.Create();
        DateTime utcNow = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        // Act
        TimeSpan delay = schedule.GetDelayUntilNextExecution(utcNow, TimeZoneInfo.Utc);

        // Assert
        Assert.Equal(TimeSpan.MaxValue, delay);
    }

    [Fact]
    public void Equals_WhenBothAreOnceAtStartupSchedules_ShouldReturnTrue()
    {
        // Act
        bool areEqual = _onceAtStartupScheduleFixture.Create().Equals(_onceAtStartupScheduleFixture.Create());

        // Assert
        Assert.True(areEqual);
    }
}
