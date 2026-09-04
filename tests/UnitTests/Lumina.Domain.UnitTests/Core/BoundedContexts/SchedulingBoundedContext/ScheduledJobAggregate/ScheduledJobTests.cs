#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobTests
{
    private readonly ScheduledJobFixture _scheduledJobFixture = new();
    private readonly IntervalScheduleFixture _intervalScheduleFixture = new();
    private readonly DailyScheduleFixture _dailyScheduleFixture = new();
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateAddedScheduledJobWithAllPropertiesSet()
    {
        // Arrange
        string name = "Scan libraries every hour";
        ScheduledTaskType taskType = ScheduledTaskType.ScanMediaLibraries;
        IntervalSchedule schedule = _intervalScheduleFixture.Create(intervalMinutes: 60);
        UserId ownerUserId = _userIdFixture.Create();

        // Act
        Result<ScheduledJob> result = ScheduledJob.Create(name, taskType, schedule, ownerUserId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(taskType, result.Value.TaskType);
        Assert.Equal(schedule, result.Value.Schedule);
        Assert.Equal(ownerUserId, result.Value.OwnerUserId);
        Assert.Equal(ScheduledJobStatus.Added, result.Value.Status);
        Assert.False(result.Value.LastStartedOnUtc.HasValue);
        Assert.False(result.Value.LastCompletedOnUtc.HasValue);
        Assert.Empty(result.Value.GetDomainEvents());
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldGenerateDistinctIds()
    {
        // Act
        Result<ScheduledJob> firstResult = ScheduledJob.Create("Job 1", ScheduledTaskType.ScanMediaLibraries, _intervalScheduleFixture.Create(), _userIdFixture.Create());
        Result<ScheduledJob> secondResult = ScheduledJob.Create("Job 2", ScheduledTaskType.ScanMediaLibraries, _intervalScheduleFixture.Create(), _userIdFixture.Create());

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        Assert.NotEqual(firstResult.Value.Id.Value, secondResult.Value.Id.Value);
    }

    [Theory]
    [InlineData(null)] // null name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void Create_WhenNameIsEmptyOrWhitespace_ShouldReturnError(string? name)
    {
        // Act
        Result<ScheduledJob> result = ScheduledJob.Create(name!, ScheduledTaskType.ScanMediaLibraries, _intervalScheduleFixture.Create(), _userIdFixture.Create());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingIdAndData_ShouldCreateScheduledJobWithThatIdAndState()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        DateTime lastStartedOnUtc = DateTime.UtcNow.AddHours(-1);
        DateTime lastCompletedOnUtc = DateTime.UtcNow.AddMinutes(-30);

        // Act
        Result<ScheduledJob> result = ScheduledJob.Create(
            _scheduledJobIdFixture.Create(id),
            "Daily cleanup",
            ScheduledTaskType.CleanTemporaryFiles,
            _dailyScheduleFixture.Create(hour: 3, minute: 0),
            _userIdFixture.Create(),
            ScheduledJobStatus.Active,
            Optional<DateTime>.Some(lastStartedOnUtc),
            Optional<DateTime>.Some(lastCompletedOnUtc));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id.Value);
        Assert.Equal(ScheduledJobStatus.Active, result.Value.Status);
        Assert.Equal(lastStartedOnUtc, result.Value.LastStartedOnUtc.Value);
        Assert.Equal(lastCompletedOnUtc, result.Value.LastCompletedOnUtc.Value);
    }

    [Fact]
    public void Add_WhenStatusIsAdded_ShouldSucceedAndRaiseAddedEvent()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();

        // Act
        Result<Success> result = scheduledJob.Add();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(ScheduledJobStatus.Added, scheduledJob.Status);
        ScheduledJobAddedDomainEvent addedEvent = Assert.IsType<ScheduledJobAddedDomainEvent>(Assert.Single(scheduledJob.GetDomainEvents()));
        Assert.Equal(scheduledJob.Id, addedEvent.ScheduledJobId);
        Assert.NotEqual(default, addedEvent.OccurredOnUtc);
    }

    [Theory]
    [InlineData(ScheduledJobStatus.Active)] // cannot add an already active scheduled job
    [InlineData(ScheduledJobStatus.Running)] // cannot add a running scheduled job
    [InlineData(ScheduledJobStatus.Completed)] // cannot add a completed scheduled job
    public void Add_WhenStatusIsNotAdded_ShouldReturnError(ScheduledJobStatus status)
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(status: status);

        // Act
        Result<Success> result = scheduledJob.Add();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobCycleAlreadyStarted, result.FirstError);
        Assert.Empty(scheduledJob.GetDomainEvents());
    }

    [Fact]
    public void StartCycle_WhenStatusIsAdded_ShouldTransitionToActiveAndRaiseCycleStartedEvent()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();

        // Act
        Result<Success> result = scheduledJob.StartCycle();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(ScheduledJobStatus.Active, scheduledJob.Status);
        ScheduledJobCycleStartedDomainEvent cycleStartedEvent = Assert.IsType<ScheduledJobCycleStartedDomainEvent>(Assert.Single(scheduledJob.GetDomainEvents()));
        Assert.Equal(scheduledJob.Id, cycleStartedEvent.ScheduledJobId);
    }

    [Theory]
    [InlineData(ScheduledJobStatus.Active)] // cannot start an already active execution cycle
    [InlineData(ScheduledJobStatus.Running)] // cannot start a running execution cycle
    public void StartCycle_WhenCycleAlreadyStarted_ShouldReturnError(ScheduledJobStatus status)
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(status: status);

        // Act
        Result<Success> result = scheduledJob.StartCycle();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobCycleAlreadyStarted, result.FirstError);
        Assert.Equal(status, scheduledJob.Status);
        Assert.Empty(scheduledJob.GetDomainEvents());
    }

    [Fact]
    public void StopCycle_WhenStatusIsActive_ShouldTransitionToAddedAndRaiseCycleStoppedEvent()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        scheduledJob.StartCycle();
        scheduledJob.GetDomainEvents();

        // Act
        Result<Success> result = scheduledJob.StopCycle();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(ScheduledJobStatus.Added, scheduledJob.Status);
        ScheduledJobCycleStoppedDomainEvent cycleStoppedEvent = Assert.IsType<ScheduledJobCycleStoppedDomainEvent>(Assert.Single(scheduledJob.GetDomainEvents()));
        Assert.Equal(scheduledJob.Id, cycleStoppedEvent.ScheduledJobId);
    }

    [Fact]
    public void StopCycle_WhenStatusIsRunning_ShouldTransitionToAddedAndRaiseExecutionStoppedEvent()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        scheduledJob.StartCycle();
        scheduledJob.MarkExecutionStarted(isCycleRun: true);
        scheduledJob.GetDomainEvents();

        // Act
        Result<Success> result = scheduledJob.StopCycle();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(ScheduledJobStatus.Added, scheduledJob.Status);
        Assert.True(scheduledJob.LastCompletedOnUtc.HasValue);
        ScheduledJobExecutionStoppedDomainEvent executionStoppedEvent = Assert.IsType<ScheduledJobExecutionStoppedDomainEvent>(Assert.Single(scheduledJob.GetDomainEvents()));
        Assert.Equal(scheduledJob.Id, executionStoppedEvent.ScheduledJobId);
    }

    [Theory]
    [InlineData(ScheduledJobStatus.Added)] // cannot stop a cycle that was never started
    [InlineData(ScheduledJobStatus.Completed)] // cannot stop a finished one time execution
    public void StopCycle_WhenCycleWasNeverStarted_ShouldReturnError(ScheduledJobStatus status)
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(status: status);

        // Act
        Result<Success> result = scheduledJob.StopCycle();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobNotStarted, result.FirstError);
        Assert.Equal(status, scheduledJob.Status);
        Assert.Empty(scheduledJob.GetDomainEvents());
    }

    [Fact]
    public void Fire_WhenStatusIsNotRunning_ShouldSucceedAndRaiseFiredEvent()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();

        // Act
        Result<Success> result = scheduledJob.Fire();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        ScheduledJobFiredDomainEvent firedEvent = Assert.IsType<ScheduledJobFiredDomainEvent>(Assert.Single(scheduledJob.GetDomainEvents()));
        Assert.Equal(scheduledJob.Id, firedEvent.ScheduledJobId);
    }

    [Fact]
    public void Fire_WhenStatusIsRunning_ShouldReturnError()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        scheduledJob.StartCycle();
        scheduledJob.MarkExecutionStarted(isCycleRun: true);
        scheduledJob.GetDomainEvents();

        // Act
        Result<Success> result = scheduledJob.Fire();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobAlreadyRunning, result.FirstError);
        Assert.Empty(scheduledJob.GetDomainEvents());
    }

    [Fact]
    public void Remove_WhenCalled_ShouldSucceedAndRaiseRemovedEvent()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();

        // Act
        Result<Success> result = scheduledJob.Remove();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        ScheduledJobRemovedDomainEvent removedEvent = Assert.IsType<ScheduledJobRemovedDomainEvent>(Assert.Single(scheduledJob.GetDomainEvents()));
        Assert.Equal(scheduledJob.Id, removedEvent.ScheduledJobId);
    }

    [Theory]
    [InlineData(true)] // cycle run
    [InlineData(false)] // one time run
    public void MarkExecutionStarted_WhenStatusIsNotRunning_ShouldTransitionToRunningAndRaiseExecutionStartedEvent(bool isCycleRun)
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();

        // Act
        Result<Success> result = scheduledJob.MarkExecutionStarted(isCycleRun);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(ScheduledJobStatus.Running, scheduledJob.Status);
        Assert.True(scheduledJob.LastStartedOnUtc.HasValue);
        ScheduledJobExecutionStartedDomainEvent startedEvent = Assert.IsType<ScheduledJobExecutionStartedDomainEvent>(Assert.Single(scheduledJob.GetDomainEvents()));
        Assert.Equal(scheduledJob.Id, startedEvent.ScheduledJobId);
        Assert.Equal(scheduledJob.TaskType, startedEvent.TaskType);
        Assert.Equal(isCycleRun, startedEvent.IsCycleRun);
        Assert.NotEqual(Guid.Empty, startedEvent.RunId);
    }

    [Fact]
    public void MarkExecutionStarted_WhenStatusIsRunning_ShouldReturnError()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        scheduledJob.StartCycle();
        scheduledJob.MarkExecutionStarted(isCycleRun: true);
        scheduledJob.GetDomainEvents();

        // Act
        Result<Success> result = scheduledJob.MarkExecutionStarted(isCycleRun: true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobAlreadyRunning, result.FirstError);
        Assert.Empty(scheduledJob.GetDomainEvents());
    }

    [Theory]
    [InlineData(true)] // a cycle run returns the scheduled job to its active status
    [InlineData(false)] // a one time execution completes the scheduled job
    public void MarkExecutionCompleted_WhenStatusIsRunning_ShouldTransitionStatusAndRaiseExecutionCompletedEvent(bool isCycleRun)
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        scheduledJob.StartCycle();
        scheduledJob.MarkExecutionStarted(isCycleRun);
        scheduledJob.GetDomainEvents();

        // Act
        Result<Success> result = scheduledJob.MarkExecutionCompleted(isCycleRun);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(isCycleRun ? ScheduledJobStatus.Active : ScheduledJobStatus.Completed, scheduledJob.Status);
        Assert.True(scheduledJob.LastCompletedOnUtc.HasValue);
        ScheduledJobExecutionCompletedDomainEvent completedEvent = Assert.IsType<ScheduledJobExecutionCompletedDomainEvent>(Assert.Single(scheduledJob.GetDomainEvents()));
        Assert.Equal(scheduledJob.Id, completedEvent.ScheduledJobId);
        Assert.Equal(scheduledJob.TaskType, completedEvent.TaskType);
        Assert.Equal(isCycleRun, completedEvent.IsCycleRun);
        Assert.NotEqual(Guid.Empty, completedEvent.RunId);
    }

    [Theory]
    [InlineData(ScheduledJobStatus.Added)] // cannot complete an execution that never started
    [InlineData(ScheduledJobStatus.Active)] // cannot complete an execution that never started
    [InlineData(ScheduledJobStatus.Completed)] // cannot complete an already completed execution
    public void MarkExecutionCompleted_WhenStatusIsNotRunning_ShouldReturnError(ScheduledJobStatus status)
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(status: status);

        // Act
        Result<Success> result = scheduledJob.MarkExecutionCompleted(isCycleRun: true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.CanOnlyCompleteRunningScheduledJob, result.FirstError);
        Assert.Empty(scheduledJob.GetDomainEvents());
    }

    [Fact]
    public void MarkExecutionCompleted_WhenLoadedFromStorageWithoutActiveRunId_ShouldReturnError()
    {
        // Arrange
        // A scheduled job loaded from the storage medium with a running status has no active run id tracked by the aggregate.
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(
            id: Guid.NewGuid(),
            status: ScheduledJobStatus.Running,
            lastStartedOnUtc: DateTime.UtcNow.AddMinutes(-5));

        // Act
        Result<Success> result = scheduledJob.MarkExecutionCompleted(isCycleRun: true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.CanOnlyCompleteRunningScheduledJob, result.FirstError);
        Assert.Empty(scheduledJob.GetDomainEvents());
    }

    [Fact]
    public void MarkExecutionCompleted_WhenCycleRunAfterExecutionStarted_ShouldAllowTheNextCycleRunToStart()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        scheduledJob.StartCycle();
        scheduledJob.MarkExecutionStarted(isCycleRun: true);
        scheduledJob.MarkExecutionCompleted(isCycleRun: true);

        // Act
        Result<Success> startNextExecutionResult = scheduledJob.MarkExecutionStarted(isCycleRun: true);
        // Assert
        Assert.False(startNextExecutionResult.IsFailure);
        Assert.Equal(ScheduledJobStatus.Running, scheduledJob.Status);
    }
}
