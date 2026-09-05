#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;

/// <summary>
/// Aggregate root for a scheduled job.
/// </summary>
[DebuggerDisplay("Id: {Id.Value}")]
public class ScheduledJob : AggregateRoot<ScheduledJobId>
{
    private Optional<Guid> _currentExecutionRunId;

    /// <summary>
    /// Gets the name of the scheduled job.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the type of the task executed by the scheduled job.
    /// </summary>
    public ScheduledTaskType TaskType { get; private set; }

    /// <summary>
    /// Gets the schedule of the scheduled job.
    /// </summary>
    public Schedule Schedule { get; private set; }

    /// <summary>
    /// Gets the status of the scheduled job.
    /// </summary>
    public ScheduledJobStatus Status { get; private set; }

    /// <summary>
    /// Gets the object representing the unique identifier of the user that owns the scheduled job.
    /// </summary>
    public UserId OwnerUserId { get; private set; }

    /// <summary>
    /// Gets the optional date and time when the task of the scheduled job last started its execution.
    /// </summary>
    public Optional<DateTime> LastStartedOnUtc { get; private set; }

    /// <summary>
    /// Gets the optional date and time when the task of the scheduled job last completed its execution.
    /// </summary>
    public Optional<DateTime> LastCompletedOnUtc { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJob"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the scheduled job.</param>
    /// <param name="name">The name of the scheduled job.</param>
    /// <param name="taskType">The type of the task executed by the scheduled job.</param>
    /// <param name="schedule">The schedule of the scheduled job.</param>
    /// <param name="ownerUserId">The object representing the unique identifier of the user that owns the scheduled job.</param>
    /// <param name="status">The status of the scheduled job.</param>
    /// <param name="lastStartedOnUtc">The optional date and time when the task of the scheduled job last started its execution.</param>
    /// <param name="lastCompletedOnUtc">The optional date and time when the task of the scheduled job last completed its execution.</param>
    private ScheduledJob(ScheduledJobId id, string name, ScheduledTaskType taskType, Schedule schedule, UserId ownerUserId, ScheduledJobStatus status, Optional<DateTime> lastStartedOnUtc, Optional<DateTime> lastCompletedOnUtc)
        : base(id)
    {
        Name = name;
        TaskType = taskType;
        Schedule = schedule;
        OwnerUserId = ownerUserId;
        Status = status;
        LastStartedOnUtc = lastStartedOnUtc;
        LastCompletedOnUtc = lastCompletedOnUtc;
        _currentExecutionRunId = Optional<Guid>.None();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ScheduledJob"/> class.
    /// </summary>
    /// <param name="name">The name of the scheduled job.</param>
    /// <param name="taskType">The type of the task executed by the scheduled job.</param>
    /// <param name="schedule">The schedule of the scheduled job.</param>
    /// <param name="ownerUserId">The object representing the unique identifier of the user that owns the scheduled job.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="ScheduledJob"/>, or an error message.
    /// </returns>
    public static Result<ScheduledJob> Create(string name, ScheduledTaskType taskType, Schedule schedule, UserId ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Scheduling.ScheduledJobNameCannotBeEmpty;
        return new ScheduledJob(ScheduledJobId.CreateUnique(), name, taskType, schedule, ownerUserId, ScheduledJobStatus.Added, Optional<DateTime>.None(), Optional<DateTime>.None());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ScheduledJob"/> class, with pre-existing data.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the scheduled job.</param>
    /// <param name="name">The name of the scheduled job.</param>
    /// <param name="taskType">The type of the task executed by the scheduled job.</param>
    /// <param name="schedule">The schedule of the scheduled job.</param>
    /// <param name="ownerUserId">The object representing the unique identifier of the user that owns the scheduled job.</param>
    /// <param name="status">The status of the scheduled job.</param>
    /// <param name="lastStartedOnUtc">The optional date and time when the task of the scheduled job last started its execution.</param>
    /// <param name="lastCompletedOnUtc">The optional date and time when the task of the scheduled job last completed its execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="ScheduledJob"/>, or an error message.
    /// </returns>
    public static Result<ScheduledJob> Create(ScheduledJobId id, string name, ScheduledTaskType taskType, Schedule schedule, UserId ownerUserId, ScheduledJobStatus status, Optional<DateTime> lastStartedOnUtc, Optional<DateTime> lastCompletedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Scheduling.ScheduledJobNameCannotBeEmpty;
        return new ScheduledJob(id, name, taskType, schedule, ownerUserId, status, lastStartedOnUtc, lastCompletedOnUtc);
    }

    /// <summary>
    /// Marks the scheduled job as added.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> Add()
    {
        if (Status != ScheduledJobStatus.Added)
            return Errors.Scheduling.ScheduledJobCycleAlreadyStarted;
        _domainEvents.Add(new ScheduledJobAddedDomainEvent(Guid.NewGuid(), Id, DateTime.UtcNow));
        return Result.Success;
    }

    /// <summary>
    /// Starts the execution cycle of the scheduled job.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> StartCycle()
    {
        if (Status == ScheduledJobStatus.Active || Status == ScheduledJobStatus.Running)
            return Errors.Scheduling.ScheduledJobCycleAlreadyStarted;

        Status = ScheduledJobStatus.Active;
        _domainEvents.Add(new ScheduledJobCycleStartedDomainEvent(Guid.NewGuid(), Id, DateTime.UtcNow));
        return Result.Success;
    }

    /// <summary>
    /// Stops the execution cycle of the scheduled job.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> StopCycle()
    {
        if (Status == ScheduledJobStatus.Added || Status == ScheduledJobStatus.Completed)
            return Errors.Scheduling.ScheduledJobNotStarted;

        ScheduledJobStatus previousStatus = Status;
        Status = ScheduledJobStatus.Added;

        // When the task of the scheduled job is currently executing, its execution is stopped as well, so its history row is closed.
        if (previousStatus == ScheduledJobStatus.Running)
        {
            LastCompletedOnUtc = Optional<DateTime>.Some(DateTime.UtcNow);
            _domainEvents.Add(new ScheduledJobExecutionStoppedDomainEvent(Guid.NewGuid(), Id, DateTime.UtcNow));
        }
        else
            _domainEvents.Add(new ScheduledJobCycleStoppedDomainEvent(Guid.NewGuid(), Id, DateTime.UtcNow));
        return Result.Success;
    }

    /// <summary>
    /// Marks the start of a one time execution of the task of the scheduled job.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> Fire()
    {
        if (Status == ScheduledJobStatus.Running)
            return Errors.Scheduling.ScheduledJobAlreadyRunning;
        _domainEvents.Add(new ScheduledJobFiredDomainEvent(Guid.NewGuid(), Id, DateTime.UtcNow));
        return Result.Success;
    }

    /// <summary>
    /// Marks the scheduled job as removed.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> Remove()
    {
        _domainEvents.Add(new ScheduledJobRemovedDomainEvent(Guid.NewGuid(), Id, DateTime.UtcNow));
        return Result.Success;
    }

    /// <summary>
    /// Marks the task of the scheduled job as started.
    /// </summary>
    /// <param name="isCycleRun">Whether the execution was triggered by the execution cycle of the scheduled job, or it is a one time execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> MarkExecutionStarted(bool isCycleRun)
    {
        if (Status == ScheduledJobStatus.Running)
            return Errors.Scheduling.ScheduledJobAlreadyRunning;

        Guid runId = Guid.NewGuid();
        _currentExecutionRunId = Optional<Guid>.Some(runId);
        Status = ScheduledJobStatus.Running;
        LastStartedOnUtc = Optional<DateTime>.Some(DateTime.UtcNow);
        _domainEvents.Add(new ScheduledJobExecutionStartedDomainEvent(Guid.NewGuid(), Id, runId, TaskType, isCycleRun, DateTime.UtcNow));
        return Result.Success;
    }

    /// <summary>
    /// Marks the task of the scheduled job as completed.
    /// </summary>
    /// <param name="isCycleRun">Whether the execution was triggered by the execution cycle of the scheduled job, or it is a one time execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> MarkExecutionCompleted(bool isCycleRun)
    {
        if (Status != ScheduledJobStatus.Running)
            return Errors.Scheduling.CanOnlyCompleteRunningScheduledJob;
        if (!_currentExecutionRunId.HasValue)
            return Errors.Scheduling.CanOnlyCompleteRunningScheduledJob;

        Guid runId = _currentExecutionRunId.Value;
        _currentExecutionRunId = Optional<Guid>.None();
        // A cycle run returns the scheduled job to its active state, where it waits for the next execution; a one time execution finishes the scheduled job.
        Status = isCycleRun ? ScheduledJobStatus.Active : ScheduledJobStatus.Completed;
        LastCompletedOnUtc = Optional<DateTime>.Some(DateTime.UtcNow);
        _domainEvents.Add(new ScheduledJobExecutionCompletedDomainEvent(Guid.NewGuid(), Id, runId, TaskType, isCycleRun, DateTime.UtcNow));
        return Result.Success;
    }
}
