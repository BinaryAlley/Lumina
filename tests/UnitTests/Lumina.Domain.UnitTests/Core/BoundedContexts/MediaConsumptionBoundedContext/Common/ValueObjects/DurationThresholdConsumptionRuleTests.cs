#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="DurationThresholdConsumptionRule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DurationThresholdConsumptionRuleTests
{
    private readonly MovieConsumptionRuleFixture _movieConsumptionRuleFixture = new();
    private readonly DurationThresholdConsumptionRule _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DurationThresholdConsumptionRuleTests"/> class.
    /// </summary>
    public DurationThresholdConsumptionRuleTests()
    {
        _sut = _movieConsumptionRuleFixture.Create();
    }

    [Fact]
    public void Counts_WhenCompletedWithZeroDuration_ShouldReturnTrue()
    {
        // Act
        bool result = _sut.Counts(ConsumptionStatus.Completed, TimeSpan.Zero, consecutivePagesRead: 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Counts_WhenStoppedWithDurationEqualToMinimum_ShouldReturnTrue()
    {
        // Arrange
        TimeSpan consumedDuration = _sut.MinimumCountedDuration;

        // Act
        bool result = _sut.Counts(ConsumptionStatus.Stopped, consumedDuration, consecutivePagesRead: 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Counts_WhenStoppedWithDurationOneTickAboveMinimum_ShouldReturnTrue()
    {
        // Arrange
        TimeSpan consumedDuration = _sut.MinimumCountedDuration + TimeSpan.FromTicks(1);

        // Act
        bool result = _sut.Counts(ConsumptionStatus.Stopped, consumedDuration, consecutivePagesRead: 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Counts_WhenStoppedWithDurationOneTickBelowMinimum_ShouldReturnFalse()
    {
        // Arrange
        TimeSpan consumedDuration = _sut.MinimumCountedDuration - TimeSpan.FromTicks(1);

        // Act
        bool result = _sut.Counts(ConsumptionStatus.Stopped, consumedDuration, consecutivePagesRead: 0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Counts_WhenInProgressWithDurationEqualToMinimum_ShouldReturnTrue()
    {
        // Arrange
        TimeSpan consumedDuration = _sut.MinimumCountedDuration;

        // Act
        bool result = _sut.Counts(ConsumptionStatus.InProgress, consumedDuration, consecutivePagesRead: 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Counts_WhenInProgressWithDurationOneTickBelowMinimum_ShouldReturnFalse()
    {
        // Arrange
        TimeSpan consumedDuration = _sut.MinimumCountedDuration - TimeSpan.FromTicks(1);

        // Act
        bool result = _sut.Counts(ConsumptionStatus.InProgress, consumedDuration, consecutivePagesRead: 0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Counts_WhenStoppedBelowMinimumWithLargePageCount_ShouldReturnFalse()
    {
        // Arrange
        TimeSpan consumedDuration = _sut.MinimumCountedDuration - TimeSpan.FromTicks(1);

        // Act
        bool result = _sut.Counts(ConsumptionStatus.Stopped, consumedDuration, consecutivePagesRead: 1000);

        // Assert
        Assert.False(result);
    }
}
