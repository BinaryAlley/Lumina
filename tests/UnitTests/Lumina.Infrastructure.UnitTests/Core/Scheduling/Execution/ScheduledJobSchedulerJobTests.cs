#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Infrastructure.Core.Scheduling.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Execution;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobSchedulerJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobSchedulerJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly ScheduledJobRuntimeRegistry _runtimeRegistry;
    private readonly ScheduledJobSchedulerJob _sut;
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobSchedulerJobTests"/> class.
    /// </summary>
    public ScheduledJobSchedulerJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScopeFactory.CreateAsyncScope().Returns(new AsyncServiceScope(_mockServiceScope));

        _runtimeRegistry = new ScheduledJobRuntimeRegistry();
        ILogger<ScheduledJobSchedulerJob> logger = Substitute.For<ILogger<ScheduledJobSchedulerJob>>();
        _sut = new ScheduledJobSchedulerJob(_runtimeRegistry, logger, _mockServiceScopeFactory);
    }

    [Fact]
    public async Task StopCycleAsync_WhenCalled_ShouldStopTheRunningCycleInTheRegistry()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource cycleCancellationTokenSource = new();
        _runtimeRegistry.TryStartCycle(scheduledJobId, cycleCancellationTokenSource);

        // Act
        await _sut.StopCycleAsync(scheduledJobId, CancellationToken.None);

        // Assert
        Assert.True(cycleCancellationTokenSource.IsCancellationRequested);
    }

    [Fact]
    public async Task StartCycleAsync_WhenCycleIsAlreadyRunning_ShouldNotStartAnotherCycle()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        using CancellationTokenSource firstCycleCancellationTokenSource = new();
        _runtimeRegistry.TryStartCycle(scheduledJobId, firstCycleCancellationTokenSource);

        // Act
        await _sut.StartCycleAsync(scheduledJobId, CancellationToken.None);
        // Wait briefly so the fire-and-forget worker, if any, gets a chance to run.
        await Task.Delay(150);

        // Assert
        using CancellationTokenSource secondCycleCancellationTokenSource = new();
        Assert.False(_runtimeRegistry.TryStartCycle(scheduledJobId, secondCycleCancellationTokenSource));
    }

    [Fact]
    public async Task RunOnceAsync_WhenCalled_ShouldCompleteWithoutThrowing()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();

        // Act
        await _sut.RunOnceAsync(scheduledJobId, CancellationToken.None);
        // Wait briefly so the fire-and-forget worker, if any, gets a chance to run.
        await Task.Delay(150);

        // Assert
        Assert.True(true);
    }
}
