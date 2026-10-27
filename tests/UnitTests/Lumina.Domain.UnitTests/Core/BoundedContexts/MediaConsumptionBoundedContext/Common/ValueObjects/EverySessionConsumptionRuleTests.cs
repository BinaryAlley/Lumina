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
/// Contains unit tests for the <see cref="EverySessionConsumptionRule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EverySessionConsumptionRuleTests
{
    private readonly PhotoConsumptionRuleFixture _photoConsumptionRuleFixture = new();
    private readonly EverySessionConsumptionRule _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="EverySessionConsumptionRuleTests"/> class.
    /// </summary>
    public EverySessionConsumptionRuleTests()
    {
        _sut = _photoConsumptionRuleFixture.Create();
    }

    [Theory]
    [InlineData(ConsumptionStatus.InProgress)] // a session that is still running
    [InlineData(ConsumptionStatus.Completed)] // a session that reached its completion
    [InlineData(ConsumptionStatus.Stopped)] // a session that ended before completion
    public void Counts_WhenSessionHasZeroDurationAndZeroPages_ShouldReturnTrue(ConsumptionStatus status)
    {
        // Act
        bool result = _sut.Counts(status, TimeSpan.Zero, consecutivePagesRead: 0);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(ConsumptionStatus.InProgress)] // a session that is still running
    [InlineData(ConsumptionStatus.Completed)] // a session that reached its completion
    [InlineData(ConsumptionStatus.Stopped)] // a session that ended before completion
    public void Counts_WhenSessionHasLargeDurationAndLargePageCount_ShouldReturnTrue(ConsumptionStatus status)
    {
        // Act
        bool result = _sut.Counts(status, TimeSpan.FromHours(5), consecutivePagesRead: 1000);

        // Assert
        Assert.True(result);
    }
}
