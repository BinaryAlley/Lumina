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
/// Contains unit tests for the <see cref="PageThresholdConsumptionRule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PageThresholdConsumptionRuleTests
{
    private readonly BookConsumptionRuleFixture _bookConsumptionRuleFixture = new();
    private readonly PageThresholdConsumptionRule _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageThresholdConsumptionRuleTests"/> class.
    /// </summary>
    public PageThresholdConsumptionRuleTests()
    {
        _sut = _bookConsumptionRuleFixture.Create();
    }

    [Fact]
    public void Counts_WhenCompletedWithZeroPages_ShouldReturnTrue()
    {
        // Act
        bool result = _sut.Counts(ConsumptionStatus.Completed, TimeSpan.Zero, consecutivePagesRead: 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Counts_WhenStoppedWithPagesEqualToMinimum_ShouldReturnFalse()
    {
        // Arrange
        int consecutivePagesRead = _sut.MinimumConsecutivePages;

        // Act
        bool result = _sut.Counts(ConsumptionStatus.Stopped, TimeSpan.Zero, consecutivePagesRead);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Counts_WhenStoppedWithPagesOneAboveMinimum_ShouldReturnTrue()
    {
        // Arrange
        int consecutivePagesRead = _sut.MinimumConsecutivePages + 1;

        // Act
        bool result = _sut.Counts(ConsumptionStatus.Stopped, TimeSpan.Zero, consecutivePagesRead);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Counts_WhenInProgressWithPagesEqualToMinimum_ShouldReturnFalse()
    {
        // Arrange
        int consecutivePagesRead = _sut.MinimumConsecutivePages;

        // Act
        bool result = _sut.Counts(ConsumptionStatus.InProgress, TimeSpan.Zero, consecutivePagesRead);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Counts_WhenInProgressWithPagesOneAboveMinimum_ShouldReturnTrue()
    {
        // Arrange
        int consecutivePagesRead = _sut.MinimumConsecutivePages + 1;

        // Act
        bool result = _sut.Counts(ConsumptionStatus.InProgress, TimeSpan.Zero, consecutivePagesRead);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Counts_WhenStoppedWithLargeDurationAndPagesEqualToMinimum_ShouldReturnFalse()
    {
        // Arrange
        TimeSpan consumedDuration = TimeSpan.FromHours(5);
        int consecutivePagesRead = _sut.MinimumConsecutivePages;

        // Act
        bool result = _sut.Counts(ConsumptionStatus.Stopped, consumedDuration, consecutivePagesRead);

        // Assert
        Assert.False(result);
    }
}
