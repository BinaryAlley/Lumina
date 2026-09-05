#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Infrastructure.Core.Scheduling.Execution;
using Lumina.Infrastructure.Core.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IScheduledJobExecutionRepository _mockScheduledJobExecutionRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly IScheduledTaskExecutorFactory _mockTaskExecutorFactory;
    private readonly IScheduledTaskExecutor _mockTaskExecutor;
    private readonly ScheduledJobRuntimeRegistry _runtimeRegistry;
    private readonly ScheduledJobSchedulerJob _sut;
    private readonly Dictionary<Type, object> _services;
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly ScheduledJobExecutionEntityFixture _scheduledJobExecutionEntityFixture = new();
    private readonly ScheduledJobFixture _scheduledJobFixture = new();
    private readonly IntervalScheduleFixture _intervalScheduleFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobSchedulerJobTests"/> class.
    /// </summary>
    public ScheduledJobSchedulerJobTests()
    {
        _services = [];
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScopeFactory.CreateAsyncScope().Returns(new AsyncServiceScope(_mockServiceScope));

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();
        _mockScheduledJobExecutionRepository = Substitute.For<IScheduledJobExecutionRepository>();
        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockUnitOfWork.ScheduledJobExecutionRepository.Returns(_mockScheduledJobExecutionRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockScheduledJobExecutionRepository.UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockTaskExecutorFactory = Substitute.For<IScheduledTaskExecutorFactory>();
        _mockTaskExecutor = Substitute.For<IScheduledTaskExecutor>();
        _mockTaskExecutor.ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockTaskExecutorFactory.CreateExecutor(Arg.Any<ScheduledTaskType>()).Returns(_mockTaskExecutor);

        _services[typeof(IUnitOfWork)] = _mockUnitOfWork;
        _services[typeof(IDomainEventPublisher)] = _mockDomainEventPublisher;
        _services[typeof(IScheduledTaskExecutorFactory)] = _mockTaskExecutorFactory;
        _mockServiceProvider.GetService(Arg.Any<Type>())
            .Returns(callInfo => _services.TryGetValue((Type)callInfo[0]!, out object? service) ? service : null);

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
    public async Task RunOnceAsync_WhenTheJobCannotStart_ShouldNotExecuteThePayload()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);

        // Act
        await _sut.RunOnceAsync(scheduledJobId, CancellationToken.None);
        // Wait briefly so the fire-and-forget worker, if any, gets a chance to run.
        await Task.Delay(150);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        Assert.True(_runtimeRegistry.TryAcquireRunSlot(scheduledJobId));
        _runtimeRegistry.ReleaseRunSlot(scheduledJobId);
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenFiredOnAJobWithoutAnActiveCycle_ShouldCompleteTheJobAsAOneTimeRun()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60));

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(domainEvent => !domainEvent.IsCycleRun), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenFiredOnAJobWithAnActiveCycle_ShouldCompleteTheJobBackToItsActiveState()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60));
        using CancellationTokenSource cycleCancellationTokenSource = new();
        _runtimeRegistry.TryStartCycle(scheduledJobId, cycleCancellationTokenSource);

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(domainEvent => domainEvent.IsCycleRun), Arg.Any<CancellationToken>());
        Assert.True(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task RunScheduledJobAsync_WhenTheStartedEventPublishingFails_ShouldReconcileTheInterruptedRunAndReleaseTheRunSlot()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob, runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: true, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);
        _mockDomainEventPublisher.When(publisher => publisher.PublishAsync(Arg.Is<ScheduledJobExecutionStartedDomainEvent>(_ => true), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("publish failed"));

        // Act
        await InvokeAsync("RunScheduledJobAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockScheduledJobRepository.Received(1).UpdateAsync(Arg.Is<ScheduledJobEntity>(entity => entity.Status == ScheduledJobStatus.Active), Arg.Any<CancellationToken>());
        await _mockScheduledJobExecutionRepository.Received(1).UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.True(_runtimeRegistry.TryAcquireRunSlot(scheduledJobId));
        _runtimeRegistry.ReleaseRunSlot(scheduledJobId);
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenTheOpenExecutionHadAnActiveCycle_ShouldRestoreTheJobToItsActiveState()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: true, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Active, result);
        await _mockScheduledJobRepository.Received(1).UpdateAsync(Arg.Is<ScheduledJobEntity>(entity => entity.Status == ScheduledJobStatus.Active), Arg.Any<CancellationToken>());
        await _mockScheduledJobExecutionRepository.Received(1).UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenTheOpenExecutionHadNoActiveCycle_ShouldRestoreTheJobToItsAddedState()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: false, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Added, result);
        await _mockScheduledJobRepository.Received(1).UpdateAsync(Arg.Is<ScheduledJobEntity>(entity => entity.Status == ScheduledJobStatus.Added), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenTheJobIsNotRunning_ShouldReturnItsCurrentStatusWithoutReconciling()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Active, result);
        await _mockScheduledJobRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCycleWorkerAsync_WhenTheJobHasAOnceAtStartupSchedule_ShouldRunTheTaskOnceAndEndTheCycle()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.OnceAtStartup);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.OnceAtStartup));

        // Act
        await InvokeAsync("RunCycleWorkerAsync", scheduledJobId, true, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(domainEvent => domainEvent.IsCycleRun), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task ResumeActiveCyclesAsync_WhenAnActiveOnceAtStartupJobExists_ShouldResumeItsCycleAndFireTheTask()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.OnceAtStartup);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(new[] { activeJob });
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.OnceAtStartup));

        // Act
        await InvokeAsync("ResumeActiveCyclesAsync", CancellationToken.None);
        await WaitUntilAsync(() => _mockTaskExecutor.ReceivedCalls().Count() > 0);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task SynchronizeBundledThemesIfNeededAsync_WhenTheScheduledJobsCannotBeRead_ShouldNotSynchronizeTheThemes()
    {
        // Arrange
        Error error = Error.Failure("ScheduledJobs.NotFound", "Failed to read the scheduled jobs");
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(error);
        IThemeService mockThemeService = Substitute.For<IThemeService>();
        _services[typeof(IThemeService)] = mockThemeService;

        // Act
        await InvokeAsync("SynchronizeBundledThemesIfNeededAsync", CancellationToken.None);

        // Assert
        mockThemeService.DidNotReceive().GetBundledThemeArchivePaths();
    }

    [Fact]
    public async Task SynchronizeBundledThemesIfNeededAsync_WhenAnActiveRepairThemesJobExists_ShouldNotSynchronizeTheThemes()
    {
        // Arrange
        ScheduledJobEntity repairThemesJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60, taskType: ScheduledTaskType.RepairThemes);
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(new[] { repairThemesJob });
        IThemeService mockThemeService = Substitute.For<IThemeService>();
        _services[typeof(IThemeService)] = mockThemeService;

        // Act
        await InvokeAsync("SynchronizeBundledThemesIfNeededAsync", CancellationToken.None);

        // Assert
        mockThemeService.DidNotReceive().GetBundledThemeArchivePaths();
    }

    [Fact]
    public async Task SynchronizeBundledThemesIfNeededAsync_WhenNoRepairThemesJobExists_ShouldSynchronizeTheBundledThemes()
    {
        // Arrange
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<ScheduledJobEntity>());
        IThemeService mockThemeService = Substitute.For<IThemeService>();
        mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        IThemeRepository mockThemeRepository = Substitute.For<IThemeRepository>();
        mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From(Enumerable.Empty<ThemeEntity>()));
        _mockUnitOfWork.ThemeRepository.Returns(mockThemeRepository);
        _services[typeof(IThemeService)] = mockThemeService;

        // Act
        await InvokeAsync("SynchronizeBundledThemesIfNeededAsync", CancellationToken.None);

        // Assert
        mockThemeService.Received(1).GetBundledThemeArchivePaths();
        await mockThemeRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeBundledThemesIfNeededAsync_WhenReadingTheScheduledJobsThrows_ShouldSwallowTheException()
    {
        // Arrange
        Task<Result<IEnumerable<ScheduledJobEntity>>> faultedRead = Task.FromException<Result<IEnumerable<ScheduledJobEntity>>>(new InvalidOperationException("The database is unavailable."));
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(faultedRead);
        IThemeService mockThemeService = Substitute.For<IThemeService>();
        _services[typeof(IThemeService)] = mockThemeService;

        // Act
        await InvokeAsync("SynchronizeBundledThemesIfNeededAsync", CancellationToken.None);

        // Assert
        mockThemeService.DidNotReceive().GetBundledThemeArchivePaths();
    }

    [Fact]
    public async Task SynchronizeBundledThemesIfNeededAsync_WhenTheThemeServiceCannotBeResolved_ShouldSwallowTheException()
    {
        // Arrange
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<ScheduledJobEntity>());
        // The theme service is intentionally not registered in the services of the scope.

        // Act
        await InvokeAsync("SynchronizeBundledThemesIfNeededAsync", CancellationToken.None);

        // Assert
        // The generic exception handler swallowed the resolution failure, so the scheduled jobs were still read.
        await _mockScheduledJobRepository.Received(1).GetActiveOrRunningAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationIsRequested_ShouldSynchronizeThemesResumeCyclesAndStopTheBackgroundService()
    {
        // Arrange
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<ScheduledJobEntity>());
        IThemeService mockThemeService = Substitute.For<IThemeService>();
        mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        IThemeRepository mockThemeRepository = Substitute.For<IThemeRepository>();
        mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From(Enumerable.Empty<ThemeEntity>()));
        _mockUnitOfWork.ThemeRepository.Returns(mockThemeRepository);
        _services[typeof(IThemeService)] = mockThemeService;
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        await InvokeAsync("ExecuteAsync", cancellationTokenSource.Token);

        // Assert
        mockThemeService.Received(1).GetBundledThemeArchivePaths();
        await _mockScheduledJobRepository.Received(2).GetActiveOrRunningAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCycleWorkerAsync_WhenTheScheduleCannotBeLoaded_ShouldEndTheCycle()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        Error error = Error.Failure("ScheduledJobs.NotFound", "Failed to read the scheduled job");
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        await InvokeAsync("RunCycleWorkerAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenCancelledBeforeItStarts_ShouldNotRunTheTask()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, cancellationTokenSource.Token);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenTheJobCannotBeRead_ShouldReturnWithoutRunningTheTask()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        Error error = Error.Failure("ScheduledJobs.NotFound", "Failed to read the scheduled job");
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenTheJobCannotBeConvertedToDomain_ShouldReturnWithoutRunningTheTask()
    {
        // Arrange
        // An interval schedule whose interval is not positive cannot be converted to its domain object.
        ScheduledJobEntity invalidJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 0);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(invalidJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(invalidJob);

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenTheTokenIsCancelledAfterTheExecutionStarted_ShouldNotRunTheTaskPayload()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob);
        using CancellationTokenSource cancellationTokenSource = new();
        _mockDomainEventPublisher.When(publisher => publisher.PublishAsync(Arg.Any<ScheduledJobExecutionStartedDomainEvent>(), Arg.Any<CancellationToken>()))
            .Do(_ => cancellationTokenSource.Cancel());

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, cancellationTokenSource.Token);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenTheTaskPayloadFails_ShouldLogTheFailureAndStillCompleteTheExecution()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60));
        _mockTaskExecutor.ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Task.Failed", "The task of the scheduled job failed."));

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(domainEvent => !domainEvent.IsCycleRun), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenTheTaskPayloadThrows_ShouldLogTheExceptionAndStillCompleteTheExecution()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60));
        _mockTaskExecutor.ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Success>>(new InvalidOperationException("The task threw.")));

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(domainEvent => !domainEvent.IsCycleRun), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenTheTaskPayloadIsCancelled_ShouldReturnWithoutCompletingTheExecution()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob);
        _mockTaskExecutor.ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Success>>(new OperationCanceledException()));

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(_ => true), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteScheduledJobRunAsync_WhenTheTokenIsCancelledAfterThePayloadRan_ShouldNotCompleteTheExecution()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob);
        using CancellationTokenSource cancellationTokenSource = new();
        _mockTaskExecutor.When(executor => executor.ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>()))
            .Do(_ => cancellationTokenSource.Cancel());
        _mockTaskExecutor.ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        await InvokeAsync("ExecuteScheduledJobRunAsync", scheduledJobId, false, cancellationTokenSource.Token);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(_ => true), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunScheduledJobAsync_WhenAnotherExecutionIsAlreadyRunning_ShouldSkipTheExecution()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob);
        Assert.True(_runtimeRegistry.TryAcquireRunSlot(scheduledJobId));

        // Act
        await InvokeAsync("RunScheduledJobAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.TryAcquireRunSlot(scheduledJobId));
        _runtimeRegistry.ReleaseRunSlot(scheduledJobId);
    }

    [Fact]
    public async Task RunScheduledJobAsync_WhenTheStartedEventPublishingIsCancelled_ShouldReleaseTheRunSlot()
    {
        // Arrange
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(addedJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(addedJob, runningJob);
        _mockDomainEventPublisher.When(publisher => publisher.PublishAsync(Arg.Any<ScheduledJobExecutionStartedDomainEvent>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new OperationCanceledException());

        // Act
        await InvokeAsync("RunScheduledJobAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        Assert.True(_runtimeRegistry.TryAcquireRunSlot(scheduledJobId));
        _runtimeRegistry.ReleaseRunSlot(scheduledJobId);
    }

    [Fact]
    public async Task CompleteExecutionAsync_WhenTheJobCannotBeReloaded_ShouldNotCompleteTheExecution()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Error.Failure("ScheduledJobs.NotFound", "Failed to read the scheduled job"));
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(id: runningJob.Id, status: ScheduledJobStatus.Added);
        scheduledJob.MarkExecutionStarted(isCycleRun: false);

        // Act
        await InvokeAsync("CompleteExecutionAsync", scheduledJob, _mockUnitOfWork, _mockDomainEventPublisher, false, CancellationToken.None);

        // Assert
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(_ => true), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteExecutionAsync_WhenTheJobIsNotRunningAnymore_ShouldNotCompleteTheExecution()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(id: runningJob.Id, status: ScheduledJobStatus.Added);
        scheduledJob.MarkExecutionStarted(isCycleRun: false);

        // Act
        await InvokeAsync("CompleteExecutionAsync", scheduledJob, _mockUnitOfWork, _mockDomainEventPublisher, false, CancellationToken.None);

        // Assert
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(_ => true), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteExecutionAsync_WhenTheExecutionCannotBeMarkedCompleted_ShouldNotPublishTheCompletionEvent()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        // The domain scheduled job was never marked as started, so its execution cannot be marked as completed.
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(id: runningJob.Id, status: ScheduledJobStatus.Active);

        // Act
        await InvokeAsync("CompleteExecutionAsync", scheduledJob, _mockUnitOfWork, _mockDomainEventPublisher, false, CancellationToken.None);

        // Assert
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(_ => true), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteExecutionAsync_WhenPublishingTheCompletionEventFails_ShouldReconcileTheInterruptedRun()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: true, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);
        _mockDomainEventPublisher.When(publisher => publisher.PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(_ => true), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("publish failed"));
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(id: runningJob.Id, status: ScheduledJobStatus.Added);
        scheduledJob.MarkExecutionStarted(isCycleRun: false);

        // Act
        await InvokeAsync("CompleteExecutionAsync", scheduledJob, _mockUnitOfWork, _mockDomainEventPublisher, false, CancellationToken.None);

        // Assert
        await _mockScheduledJobRepository.Received(1).UpdateAsync(Arg.Is<ScheduledJobEntity>(entity => entity.Status == ScheduledJobStatus.Active), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenTheJobCannotBeRead_ShouldReturnAdded()
    {
        // Arrange
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("ScheduledJobs.NotFound", "Failed to read the scheduled job"));

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Added, result);
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenTheOpenExecutionCannotBeRead_ShouldReturnAdded()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("ScheduledJobExecutions.NotFound", "Failed to read the open execution"));

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Added, result);
        await _mockScheduledJobRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteExecutionAsync_WhenPublishingTheCompletionEventIsCancelled_ShouldSwallowTheCancellation()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        _mockDomainEventPublisher.When(publisher => publisher.PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(_ => true), Arg.Any<CancellationToken>()))
            .Do(_ => throw new OperationCanceledException());
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(id: runningJob.Id, status: ScheduledJobStatus.Added);
        scheduledJob.MarkExecutionStarted(isCycleRun: false);

        // Act
        await InvokeAsync("CompleteExecutionAsync", scheduledJob, _mockUnitOfWork, _mockDomainEventPublisher, false, CancellationToken.None);

        // Assert
        // A cancelled completion is a normal shutdown outcome, so no reconciliation is started.
        await _mockScheduledJobRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenThereIsNoOpenExecution_ShouldRestoreTheJobToItsActiveState()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(null));

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Active, result);
        await _mockScheduledJobRepository.Received(1).UpdateAsync(Arg.Is<ScheduledJobEntity>(entity => entity.Status == ScheduledJobStatus.Active), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenUpdatingTheScheduledJobFails_ShouldReturnAdded()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: true, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("ScheduledJobs.UpdateFailed", "Failed to update the scheduled job"));

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Added, result);
        await _mockScheduledJobExecutionRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenClosingTheOpenExecutionFails_ShouldReturnAdded()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: true, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);
        _mockScheduledJobExecutionRepository.UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("ScheduledJobExecutions.UpdateFailed", "Failed to update the execution"));

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Added, result);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenSavingTheReconciliationThrows_ShouldLogAndReturnAdded()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: true, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(new InvalidOperationException("Save failed")));

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Added, result);
    }

    [Fact]
    public async Task ReconcileInterruptedRunAsync_WhenSavingTheReconciliationIsCancelled_ShouldReturnAdded()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: true, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(new OperationCanceledException()));

        // Act
        ScheduledJobStatus result = await InvokeAsync<ScheduledJobStatus>("ReconcileInterruptedRunAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Equal(ScheduledJobStatus.Added, result);
    }

    [Fact]
    public async Task ResumeActiveCyclesAsync_WhenTheScheduledJobsCannotBeRead_ShouldNotResumeAnyCycle()
    {
        // Arrange
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure("ScheduledJobs.NotFound", "Failed to read the scheduled jobs"));

        // Act
        await InvokeAsync("ResumeActiveCyclesAsync", CancellationToken.None);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResumeActiveCyclesAsync_WhenARunningJobWasReconciledToItsAddedState_ShouldSkipResumingItsCycle()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(new[] { runningJob });
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: false, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);

        // Act
        await InvokeAsync("ResumeActiveCyclesAsync", CancellationToken.None);
        await Task.Delay(100);

        // Assert
        await _mockScheduledJobRepository.Received(1).UpdateAsync(Arg.Is<ScheduledJobEntity>(entity => entity.Status == ScheduledJobStatus.Added), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task ResumeActiveCyclesAsync_WhenARunningJobWasReconciledToItsActiveState_ShouldResumeItsCycleAndFireTheTask()
    {
        // Arrange
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.OnceAtStartup);
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.OnceAtStartup);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(runningJob.Id);
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(new[] { runningJob });
        // The first read reconciles the interrupted run while the job is still running; the reads made by the resumed cycle observe the reconciled active status.
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob, activeJob, activeJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.OnceAtStartup));
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: runningJob.Id, isCycleRun: false, wasCycleActive: true, completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(openExecution);

        // Act
        await InvokeAsync("ResumeActiveCyclesAsync", CancellationToken.None);
        await WaitUntilAsync(() => _mockTaskExecutor.ReceivedCalls().Count() > 0);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResumeActiveCyclesAsync_WhenAnActiveIntervalJobExists_ShouldResumeItsCycleWithoutFiringTheTask()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetActiveOrRunningAsync(Arg.Any<CancellationToken>()).Returns(new[] { activeJob });
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);

        // Act
        await InvokeAsync("ResumeActiveCyclesAsync", CancellationToken.None);
        await WaitUntilAsync(() => _runtimeRegistry.HasActiveCycle(scheduledJobId));
        await _sut.StopCycleAsync(scheduledJobId, CancellationToken.None);
        await Task.Delay(50);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCycleWorkerAsync_WhenTheTimerTicks_ShouldRunTheTaskAfterEachTickAndEndTheCycle()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        RegisterDateTimeProvider();
        IScheduledJobCycleTicker mockCycleTicker = Substitute.For<IScheduledJobCycleTicker>();
        mockCycleTicker.WaitForNextTickAsync(Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(true), new ValueTask<bool>(true), new ValueTask<bool>(false));
        ScheduledJobSchedulerJob schedulerJob = CreateScheduledJobSchedulerJob(_ => mockCycleTicker);

        // Act
        await InvokeOnAsync(schedulerJob, "RunCycleWorkerAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.Received(2).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(2).PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(domainEvent => domainEvent.IsCycleRun), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task RunCycleWorkerAsync_WhenTheTimerTicksForADailySchedule_ShouldRecalculateThePeriodAfterEveryRun()
    {
        // Arrange
        ScheduledJobEntity dailyJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.DailyAtHourAndMinute, hour: 10, minute: 30);
        ScheduledJobEntity runningDailyJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.DailyAtHourAndMinute, hour: 10, minute: 30);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(dailyJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(dailyJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningDailyJob);
        RegisterDateTimeProvider();
        IScheduledJobCycleTicker mockCycleTicker = Substitute.For<IScheduledJobCycleTicker>();
        mockCycleTicker.WaitForNextTickAsync(Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(true), new ValueTask<bool>(false));
        ScheduledJobSchedulerJob schedulerJob = CreateScheduledJobSchedulerJob(_ => mockCycleTicker);

        // Act
        await InvokeOnAsync(schedulerJob, "RunCycleWorkerAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        mockCycleTicker.Received(1).Period = Arg.Any<TimeSpan>();
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task RunCycleWorkerAsync_WhenTheCycleIsStoppedDuringARun_ShouldEndTheCycleOnTheNextTick()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        _mockScheduledJobRepository.GetByIdWithoutTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(runningJob);
        RegisterDateTimeProvider();
        _mockTaskExecutor.ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success)
            .AndDoes(_ => _runtimeRegistry.StopCycle(scheduledJobId));
        IScheduledJobCycleTicker mockCycleTicker = Substitute.For<IScheduledJobCycleTicker>();
        mockCycleTicker.WaitForNextTickAsync(Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(true), new ValueTask<bool>(true), new ValueTask<bool>(false));
        ScheduledJobSchedulerJob schedulerJob = CreateScheduledJobSchedulerJob(_ => mockCycleTicker);

        // Act
        await InvokeOnAsync(schedulerJob, "RunCycleWorkerAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.Received(1).ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Is<ScheduledJobExecutionCompletedDomainEvent>(_ => true), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task RunCycleWorkerAsync_WhenTheCycleTokenIsCancelled_ShouldSwallowTheCancellationAndEndTheCycle()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        RegisterDateTimeProvider();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        await InvokeAsync("RunCycleWorkerAsync", scheduledJobId, false, cancellationTokenSource.Token);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task RunCycleWorkerAsync_WhenWaitingForTheNextTickThrows_ShouldLogTheErrorAndEndTheCycle()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(activeJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activeJob);
        RegisterDateTimeProvider();
        IScheduledJobCycleTicker mockCycleTicker = Substitute.For<IScheduledJobCycleTicker>();
        mockCycleTicker.WaitForNextTickAsync(Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(Task.FromException<bool>(new InvalidOperationException("The ticker failed."))));
        ScheduledJobSchedulerJob schedulerJob = CreateScheduledJobSchedulerJob(_ => mockCycleTicker);

        // Act
        await InvokeOnAsync(schedulerJob, "RunCycleWorkerAsync", scheduledJobId, false, CancellationToken.None);

        // Assert
        await _mockTaskExecutor.DidNotReceive().ExecutePayloadAsync(Arg.Any<ScheduledJob>(), Arg.Any<CancellationToken>());
        Assert.False(_runtimeRegistry.HasActiveCycle(scheduledJobId));
    }

    [Fact]
    public async Task CalculateDelayAsync_WhenCalledWithAnIntervalSchedule_ShouldReturnTheIntervalDelay()
    {
        // Arrange
        IntervalSchedule intervalSchedule = _intervalScheduleFixture.Create(intervalMinutes: 30);
        IDateTimeProvider mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        mockDateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        _services[typeof(IDateTimeProvider)] = mockDateTimeProvider;

        // Act
        TimeSpan result = await InvokeAsync<TimeSpan>("CalculateDelayAsync", intervalSchedule);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(30), result);
    }

    [Fact]
    public async Task GetScheduleAsync_WhenTheScheduledJobCannotBeConvertedToDomain_ShouldReturnNull()
    {
        // Arrange
        // An interval schedule whose interval is not positive cannot be converted to its domain object.
        ScheduledJobEntity invalidJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active, scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 0);
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create(invalidJob.Id);
        _mockScheduledJobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(invalidJob);

        // Act
        Schedule? result = await InvokeAsync<Schedule?>("GetScheduleAsync", scheduledJobId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Registers a date time provider that returns a fixed UTC time in the services of the mocked scope.
    /// </summary>
    private void RegisterDateTimeProvider()
    {
        IDateTimeProvider mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        mockDateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        _services[typeof(IDateTimeProvider)] = mockDateTimeProvider;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ScheduledJobSchedulerJob"/> class whose cycle cadence is driven by the tickers created by <paramref name="cycleTickerFactory"/>.
    /// </summary>
    /// <param name="cycleTickerFactory">The factory that creates the ticker driving the cadence of an execution cycle.</param>
    /// <returns>The created scheduled job scheduler.</returns>
    private ScheduledJobSchedulerJob CreateScheduledJobSchedulerJob(Func<TimeSpan, IScheduledJobCycleTicker> cycleTickerFactory)
    {
        ILogger<ScheduledJobSchedulerJob> logger = Substitute.For<ILogger<ScheduledJobSchedulerJob>>();
        return new ScheduledJobSchedulerJob(_runtimeRegistry, logger, _mockServiceScopeFactory, cycleTickerFactory);
    }

    /// <summary>
    /// Invokes the private method named <paramref name="methodName"/> on <paramref name="schedulerJob"/> and awaits its returned task.
    /// </summary>
    /// <param name="schedulerJob">The scheduled job scheduler on which the private method is invoked.</param>
    /// <param name="methodName">The name of the private method to invoke.</param>
    /// <param name="args">The arguments of the method.</param>
    private static async Task InvokeOnAsync(ScheduledJobSchedulerJob schedulerJob, string methodName, params object?[] args)
    {
        Task task = (Task)GetPrivateMethod(methodName).Invoke(schedulerJob, args)!;
        await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Invokes the private method named <paramref name="methodName"/> and awaits its returned task.
    /// </summary>
    /// <param name="methodName">The name of the private method to invoke.</param>
    /// <param name="args">The arguments of the method.</param>
    private async Task InvokeAsync(string methodName, params object?[] args)
    {
        Task task = (Task)GetPrivateMethod(methodName).Invoke(_sut, args)!;
        await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Invokes the private method named <paramref name="methodName"/> and awaits its returned task of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="methodName">The name of the private method to invoke.</param>
    /// <param name="args">The arguments of the method.</param>
    /// <typeparam name="T">The type of the value the task returns.</typeparam>
    /// <returns>The value returned by the method.</returns>
    private async Task<T> InvokeAsync<T>(string methodName, params object?[] args)
    {
        Task<T> task = (Task<T>)GetPrivateMethod(methodName).Invoke(_sut, args)!;
        return await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the private instance method named <paramref name="methodName"/>.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <returns>The reflected method.</returns>
    private static MethodInfo GetPrivateMethod(string methodName)
    {
        return typeof(ScheduledJobSchedulerJob).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"The method '{methodName}' was not found on the scheduled job scheduler.");
    }

    /// <summary>
    /// Waits until the provided condition is met, or the timeout elapses.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        int remainingAttempts = 200;
        while (!condition() && remainingAttempts > 0)
        {
            remainingAttempts--;
            await Task.Delay(10);
        }
    }
}
