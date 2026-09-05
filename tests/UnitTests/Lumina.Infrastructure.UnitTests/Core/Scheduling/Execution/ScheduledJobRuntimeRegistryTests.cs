#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Infrastructure.Core.Scheduling.Execution;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Execution;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobRuntimeRegistry"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobRuntimeRegistryTests
{
    private readonly ScheduledJobRuntimeRegistry _sut = new();
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();

    [Fact]
    public void TryStartCycle_WhenNoCycleIsRunning_ShouldReturnTrue()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource cycleCancellationTokenSource = new();

        // Act
        bool result = _sut.TryStartCycle(scheduledJobId, cycleCancellationTokenSource);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryStartCycle_WhenCycleIsAlreadyRunning_ShouldReturnFalse()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource firstCancellationTokenSource = new();
        using CancellationTokenSource secondCancellationTokenSource = new();
        _sut.TryStartCycle(scheduledJobId, firstCancellationTokenSource);

        // Act
        bool result = _sut.TryStartCycle(scheduledJobId, secondCancellationTokenSource);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StopCycle_WhenCycleIsRunning_ShouldCancelTheCycleAndAllowRestartingIt()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource cycleCancellationTokenSource = new();
        _sut.TryStartCycle(scheduledJobId, cycleCancellationTokenSource);

        // Act
        _sut.StopCycle(scheduledJobId);

        // Assert
        Assert.True(cycleCancellationTokenSource.IsCancellationRequested);
        using CancellationTokenSource restartedCycleCancellationTokenSource = new();
        Assert.True(_sut.TryStartCycle(scheduledJobId, restartedCycleCancellationTokenSource));
    }

    [Fact]
    public void StopCycle_WhenNoCycleIsRunning_ShouldNotThrow()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();

        // Act
        _sut.StopCycle(scheduledJobId);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void TryAcquireRunSlot_WhenNoExecutionIsRunning_ShouldReturnTrueAndThenFalseWhileHeld()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();

        // Act
        bool firstAcquire = _sut.TryAcquireRunSlot(scheduledJobId);
        bool secondAcquire = _sut.TryAcquireRunSlot(scheduledJobId);

        // Assert
        Assert.True(firstAcquire);
        Assert.False(secondAcquire);
    }

    [Fact]
    public void ReleaseRunSlot_WhenSlotWasAcquired_ShouldAllowAcquiringItAgain()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        _sut.TryAcquireRunSlot(scheduledJobId);

        // Act
        _sut.ReleaseRunSlot(scheduledJobId);

        // Assert
        Assert.True(_sut.TryAcquireRunSlot(scheduledJobId));
    }

    [Fact]
    public void TryAcquireRunSlot_AfterReleasingTheSlot_ShouldReturnTrue()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        _sut.TryAcquireRunSlot(scheduledJobId);
        _sut.ReleaseRunSlot(scheduledJobId);

        // Act
        bool result = _sut.TryAcquireRunSlot(scheduledJobId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasActiveCycle_WhenCycleIsRunning_ShouldReturnTrue()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource cycleCancellationTokenSource = new();
        _sut.TryStartCycle(scheduledJobId, cycleCancellationTokenSource);

        // Act
        bool result = _sut.HasActiveCycle(scheduledJobId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasActiveCycle_WhenNoCycleIsRunning_ShouldReturnFalse()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();

        // Act
        bool result = _sut.HasActiveCycle(scheduledJobId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasActiveCycle_AfterTheCycleWasStopped_ShouldReturnFalse()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource cycleCancellationTokenSource = new();
        _sut.TryStartCycle(scheduledJobId, cycleCancellationTokenSource);
        _sut.StopCycle(scheduledJobId);

        // Act
        bool result = _sut.HasActiveCycle(scheduledJobId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EndCycle_WhenCalledWithTheOwnedTokenSource_ShouldCancelItAndClearTheCycle()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource cycleCancellationTokenSource = new();
        _sut.TryStartCycle(scheduledJobId, cycleCancellationTokenSource);

        // Act
        _sut.EndCycle(scheduledJobId, cycleCancellationTokenSource);

        // Assert
        Assert.True(cycleCancellationTokenSource.IsCancellationRequested);
        Assert.False(_sut.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public void EndCycle_WhenTheCycleWasReplacedByANewerOne_ShouldNotCancelTheNewerCycle()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource oldCycleCancellationTokenSource = new();
        using CancellationTokenSource newCycleCancellationTokenSource = new();
        _sut.TryStartCycle(scheduledJobId, oldCycleCancellationTokenSource);
        _sut.StopCycle(scheduledJobId);
        _sut.TryStartCycle(scheduledJobId, newCycleCancellationTokenSource);

        // Act
        _sut.EndCycle(scheduledJobId, oldCycleCancellationTokenSource);

        // Assert
        Assert.True(oldCycleCancellationTokenSource.IsCancellationRequested);
        Assert.False(newCycleCancellationTokenSource.IsCancellationRequested);
        Assert.True(_sut.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public void EndCycle_WhenNoCycleIsRegistered_ShouldStillCancelTheOwnedTokenSource()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource cycleCancellationTokenSource = new();

        // Act
        _sut.EndCycle(scheduledJobId, cycleCancellationTokenSource);

        // Assert
        Assert.True(cycleCancellationTokenSource.IsCancellationRequested);
        Assert.False(_sut.HasActiveCycle(scheduledJobId));
    }
}
