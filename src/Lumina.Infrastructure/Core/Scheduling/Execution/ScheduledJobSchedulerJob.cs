#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Scheduling;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution;

/// <summary>
/// Background service that schedules and executes the tasks of the scheduled jobs.
/// </summary>
public sealed class ScheduledJobSchedulerJob : BackgroundService, IScheduledJobScheduler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IScheduledJobRuntimeRegistry _runtimeRegistry;
    private readonly ILogger<ScheduledJobSchedulerJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobSchedulerJob"/> class.
    /// </summary>
    /// <param name="runtimeRegistry">Injected registry that holds the live runtime state of the scheduled jobs.</param>
    /// <param name="logger">Injected service for logging.</param>
    /// <param name="serviceScopeFactory">Injected factory used for creating scopes in which services are requested.</param>
    public ScheduledJobSchedulerJob(IScheduledJobRuntimeRegistry runtimeRegistry, ILogger<ScheduledJobSchedulerJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _runtimeRegistry = runtimeRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Starts the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution cycle is started.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task StartCycleAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken)
    {
        RunCycleWorkerAsync(scheduledJobId, runImmediately: true, cancellationToken).FireAndForgetSafeAsync();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution cycle is stopped.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task StopCycleAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken)
    {
        _runtimeRegistry.StopCycle(scheduledJobId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Fires the task of the scheduled job identified by <paramref name="scheduledJobId"/> once, without affecting its execution cycle.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose task is fired.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RunOnceAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken)
    {
        RunOnceWorkerAsync(scheduledJobId, cancellationToken).FireAndForgetSafeAsync();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Method called when the background service starts; resumes the execution cycles of the scheduled jobs whose cycle was active.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token that can be used to stop the execution.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ResumeActiveCyclesAsync(stoppingToken).ConfigureAwait(false);
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Resumes the execution cycles of the scheduled jobs whose cycle was active when the application shut down.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task ResumeActiveCyclesAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        Result<IEnumerable<ScheduledJobEntity>> getScheduledJobsResult = await unitOfWork.ScheduledJobRepository.GetActiveOrRunningAsync(cancellationToken).ConfigureAwait(false);
        if (getScheduledJobsResult.IsFailure)
        {
            _logger.LogWarning("Failed to read the scheduled jobs to resume: {Error}", getScheduledJobsResult.FirstError.Description);
            return;
        }
        foreach (ScheduledJobEntity scheduledJob in getScheduledJobsResult.Value)
        {
            ScheduledJobId scheduledJobId = ScheduledJobId.Create(scheduledJob.Id);
            // A scheduled job that was running when the application shut down had its execution interrupted, so its open
            // execution history row is closed and its status is reset before its cycle is resumed.
            if (scheduledJob.Status == ScheduledJobStatus.Running)
            {
                ScheduledJobStatus reconciledStatus = await ReconcileInterruptedRunAsync(scheduledJobId, cancellationToken).ConfigureAwait(false);
                if (reconciledStatus != ScheduledJobStatus.Active)
                    continue;
            }
            // A once at startup scheduled job fires immediately when its cycle is resumed at startup; the other scheduled jobs wait for their next scheduled execution.
            bool runImmediately = scheduledJob.ScheduleType == ScheduleType.OnceAtStartup;
            RunCycleWorkerAsync(scheduledJobId, runImmediately, cancellationToken).FireAndForgetSafeAsync();
        }
    }

    /// <summary>
    /// Runs the execution cycle worker of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is run.</param>
    /// <param name="runImmediately">Whether the task of the scheduled job is run immediately when the cycle starts.</param>
    /// <param name="stoppingToken">Cancellation token that can be used to stop the execution.</param>
    private async Task RunCycleWorkerAsync(ScheduledJobId scheduledJobId, bool runImmediately, CancellationToken stoppingToken)
    {
        // Each cycle owns its own cancellation token source: it is the handle the runtime registry stores, so stopping one
        // scheduled job cancels exactly this cycle and no other job's cycle, even though the cycle worker itself runs "fire and forget" on another thread.
        using CancellationTokenSource cycleCancellationTokenSource = new();
        // A scheduled job has at most one live cycle, and starts are "fire and forget", so when the registry already holds a
        // cycle for the job, there is nothing for this worker to do and the existing cycle keeps ticking.
        if (!_runtimeRegistry.TryStartCycle(scheduledJobId, cycleCancellationTokenSource))
            return;

        try
        {
            // The cycle token is cancelled when the application shuts down (through the host stopping token) or when this cycle is stopped
            // (through its own token source), so a single token lets every await of the cycle observe both stop paths.
            using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cycleCancellationTokenSource.Token);
            CancellationToken cycleToken = linkedCancellationTokenSource.Token;
            // An immediate run is executed as a cycle run, so it completes the scheduled job back to its active state and is
            // recorded as part of the cycle; this is the path taken by an admin start, by the resume at startup and by the seed of the default jobs.
            if (runImmediately)
                await RunScheduledJobAsync(scheduledJobId, isCycleRun: true, cycleToken).ConfigureAwait(false);

            // The schedule is read from the storage medium for every cycle, so the delay until the next run is always
            // computed from the persisted schedule; when the scheduled job can no longer be read, the cycle simply ends.
            Schedule? schedule = await GetScheduleAsync(scheduledJobId, cycleToken).ConfigureAwait(false);
            if (schedule is null)
                return;
            // A once at startup schedule fires its task exactly once when its cycle starts, at the application startup; the cycle
            // then ends, and the scheduled job stays active, so its cycle is started again at the next application startup.
            if (schedule.ScheduleType == ScheduleType.OnceAtStartup)
                return;

            TimeSpan delay = await CalculateDelayAsync(schedule).ConfigureAwait(false);
            // The periodic timer owns the cadence of the cycle: an interval schedule ticks every interval, and a daily
            // schedule ticks after the delay until the next daily hour and minute.
            using PeriodicTimer timer = new(delay);
            // Each iteration waits for the next tick, runs the task and loops; a cancelled token or a disposed timer makes WaitForNextTickAsync exit,
            // which ends the cycle through the finally block below.
            while (await timer.WaitForNextTickAsync(cycleToken).ConfigureAwait(false))
            {
                if (cycleToken.IsCancellationRequested)
                    return;
                // The run is awaited inside the tick and the periodic timer does not queue missed ticks, so a task that
                // outlasts its interval drops the missed ticks instead of piling them up; the run slot acquired inside
                // RunScheduledJobAsync is the second guard against overlapping executions.
                await RunScheduledJobAsync(scheduledJobId, isCycleRun: true, cycleToken).ConfigureAwait(false);
                // A daily schedule needs its delay recalculated after every run from the current time, so the next run
                // happens at the next day's hour and minute, also across daylight saving time changes.
                if (schedule.ScheduleType == ScheduleType.DailyAtHourAndMinute)
                    timer.Period = await CalculateDelayAsync(schedule).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // A cancelled cycle is the normal way the loop ends when the cycle is stopped or the application shuts down, so it is not an error and must not be logged as one.
        }
        catch (Exception exception)
        {
            // Failures of a single run are handled inside RunScheduledJobAsync, so an exception reaching this point is unexpected and ends the cycle.
            _logger.LogError(exception, "The execution cycle of the scheduled job '{ScheduledJobId}' failed.", scheduledJobId.Value);
        }
        finally
        {
            // The registry is always told that the cycle ended, because every exit path passes through here, including the
            // early returns above; EndCycle cancels and deregisters the cycle, and the outer using disposes the token source.
            _runtimeRegistry.EndCycle(scheduledJobId, cycleCancellationTokenSource);
        }
    }

    /// <summary>
    /// Runs the task of the scheduled job identified by <paramref name="scheduledJobId"/> once.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose task is run.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task RunOnceWorkerAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken)
    {
        try
        {
            await RunScheduledJobAsync(scheduledJobId, isCycleRun: false, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The one time execution of the scheduled job '{ScheduledJobId}' failed.", scheduledJobId.Value);
        }
    }

    /// <summary>
    /// Runs the task of the scheduled job identified by <paramref name="scheduledJobId"/>, guarding it against overlapping executions.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose task is run.</param>
    /// <param name="isCycleRun">Whether the execution was triggered by the execution cycle of the scheduled job, or it is a one time execution.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task RunScheduledJobAsync(ScheduledJobId scheduledJobId, bool isCycleRun, CancellationToken cancellationToken)
    {
        // Skip the execution when another execution of the same scheduled job is already running.
        if (!_runtimeRegistry.TryAcquireRunSlot(scheduledJobId))
            return;
        try
        {
            await ExecuteScheduledJobRunAsync(scheduledJobId, isCycleRun, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // An execution that was cancelled is a normal outcome when the cycle of the scheduled job is stopped or the application shuts down.
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The execution of the scheduled job '{ScheduledJobId}' failed unexpectedly.", scheduledJobId.Value);
            await ReconcileInterruptedRunAsync(scheduledJobId, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            _runtimeRegistry.ReleaseRunSlot(scheduledJobId);
        }
    }

    /// <summary>
    /// Executes a single run of the task of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose task is run.</param>
    /// <param name="isCycleRun">Whether the execution was triggered by the execution cycle of the scheduled job, or it is a one time execution.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task ExecuteScheduledJobRunAsync(ScheduledJobId scheduledJobId, bool isCycleRun, CancellationToken cancellationToken)
    {
        // A run that was requested to stop before it started must not start at all.
        if (cancellationToken.IsCancellationRequested)
            return;

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        IUnitOfWork unitOfWork = services.GetRequiredService<IUnitOfWork>();
        IDomainEventPublisher domainEventPublisher = services.GetRequiredService<IDomainEventPublisher>();

        // Load the scheduled job, and mark its task as started.
        Result<ScheduledJobEntity?> getScheduledJobResult = await unitOfWork.ScheduledJobRepository.GetByIdAsync(scheduledJobId.Value, cancellationToken).ConfigureAwait(false);
        if (getScheduledJobResult.IsFailure || getScheduledJobResult.Value is null)
            return;
        Result<ScheduledJob> scheduledJobDomainResult = getScheduledJobResult.Value.ToDomainEntity();
        if (scheduledJobDomainResult.IsFailure)
            return;
        ScheduledJob scheduledJob = scheduledJobDomainResult.Value;
        Result<Success> startExecutionResult = scheduledJob.MarkExecutionStarted(isCycleRun);
        if (startExecutionResult.IsFailure)
            return;
        foreach (IDomainEvent domainEvent in scheduledJob.GetDomainEvents())
            await domainEventPublisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);

        // Execute the payload of the task of the scheduled job; a payload failure is a normal outcome that is logged, while a
        // thrown exception is logged as well, so that the run is still completed instead of leaving the scheduled job running.
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            IScheduledTaskExecutorFactory taskExecutorFactory = services.GetRequiredService<IScheduledTaskExecutorFactory>();
            IScheduledTaskExecutor taskExecutor = taskExecutorFactory.CreateExecutor(scheduledJob.TaskType);
            Result<Success> executePayloadResult = await taskExecutor.ExecutePayloadAsync(scheduledJob, cancellationToken).ConfigureAwait(false);
            if (executePayloadResult.IsFailure)
                _logger.LogWarning("The task of the scheduled job '{ScheduledJobName}' failed: {Error}", scheduledJob.Name, executePayloadResult.FirstError.Description);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The task of the scheduled job '{ScheduledJobName}' threw an exception.", scheduledJob.Name);
        }

        if (cancellationToken.IsCancellationRequested)
            return;

        await CompleteExecutionAsync(scheduledJob, unitOfWork, domainEventPublisher, isCycleRun, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Completes the execution of the task of <paramref name="scheduledJob"/>, when the scheduled job is still running.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job whose task execution is completed.</param>
    /// <param name="unitOfWork">The unit of work of the current run, whose tracked entities are reused by the completion handlers.</param>
    /// <param name="domainEventPublisher">The publisher used to publish the completion domain events.</param>
    /// <param name="isCycleRun">Whether the execution was triggered by the execution cycle of the scheduled job, or it is a one time execution.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task CompleteExecutionAsync(ScheduledJob scheduledJob, IUnitOfWork unitOfWork, IDomainEventPublisher domainEventPublisher, bool isCycleRun, CancellationToken cancellationToken)
    {
        try
        {
            // Reload the scheduled job from the storage medium without tracking, because a stop or a removal that happened while the
            // task was executing already reset its state, and completing the execution here would silently undo that change.
            Result<ScheduledJobEntity?> getScheduledJobResult = await unitOfWork.ScheduledJobRepository.GetByIdWithoutTrackingAsync(scheduledJob.Id.Value, cancellationToken).ConfigureAwait(false);
            if (getScheduledJobResult.IsFailure || getScheduledJobResult.Value is null)
                return;
            if (getScheduledJobResult.Value.Status != ScheduledJobStatus.Running)
                return;

            // A one time execution returns the scheduled job to its active state when its execution cycle is still running, so the
            // running cycle keeps scheduling executions and the job can still be stopped; the same applies to a once at startup job,
            // whose active state is what makes it fire again at the next application startup.
            bool completesTheCycleRun = isCycleRun || _runtimeRegistry.HasActiveCycle(scheduledJob.Id) || scheduledJob.Schedule.ScheduleType == ScheduleType.OnceAtStartup;
            Result<Success> completeExecutionResult = scheduledJob.MarkExecutionCompleted(completesTheCycleRun);
            if (completeExecutionResult.IsFailure)
                return;
            foreach (IDomainEvent domainEvent in scheduledJob.GetDomainEvents())
                await domainEventPublisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            // The completion failed after the execution was persisted as running, so the interrupted run is reconciled directly.
            _logger.LogError(exception, "Failed to complete the execution of the scheduled job '{ScheduledJobId}'.", scheduledJob.Id.Value);
            await ReconcileInterruptedRunAsync(scheduledJob.Id, CancellationToken.None).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reconciles the scheduled job identified by <paramref name="scheduledJobId"/> when its execution was interrupted: its open
    /// execution history row is closed, and the scheduled job is returned to a status from which it can run again.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose interrupted execution is reconciled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The status the scheduled job was reconciled to, or its current status when no reconciliation was needed.</returns>
    private async Task<ScheduledJobStatus> ReconcileInterruptedRunAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            Result<ScheduledJobEntity?> getScheduledJobResult = await unitOfWork.ScheduledJobRepository.GetByIdAsync(scheduledJobId.Value, cancellationToken).ConfigureAwait(false);
            if (getScheduledJobResult.IsFailure || getScheduledJobResult.Value is null)
                return ScheduledJobStatus.Added;
            ScheduledJobEntity scheduledJob = getScheduledJobResult.Value;
            // A scheduled job that is not running needs no reconciliation.
            if (scheduledJob.Status != ScheduledJobStatus.Running)
                return scheduledJob.Status;

            // An interrupted execution returns the scheduled job to its active state when its execution cycle was active
            // when the execution started, so its cycle is resumed at startup; an interrupted execution of a job whose cycle
            // was not active returns it to its added state. The cycle active state is read from the execution history row
            // and not from the IsCycleRun flag, so that a manually fired one time execution on an active job keeps its
            // manual audit trail while its cycle is still resumed.
            Result<ScheduledJobExecutionEntity?> getOpenExecutionResult = await unitOfWork.ScheduledJobExecutionRepository
                .GetOpenByScheduledJobIdAsync(scheduledJob.Id, cancellationToken).ConfigureAwait(false);
            if (getOpenExecutionResult.IsFailure)
                return ScheduledJobStatus.Added;
            ScheduledJobExecutionEntity? openExecution = getOpenExecutionResult.Value;
            ScheduledJobStatus reconciledStatus = (openExecution?.WasCycleActive ?? true) ? ScheduledJobStatus.Active : ScheduledJobStatus.Added;

            Result<Updated> updateScheduledJobResult = await unitOfWork.ScheduledJobRepository.UpdateAsync(
                CreateUpdatedScheduledJob(scheduledJob, reconciledStatus, scheduledJob.LastStartedOnUtc, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
            if (updateScheduledJobResult.IsFailure)
                return ScheduledJobStatus.Added;
            if (openExecution is not null)
            {
                Result<Updated> updateExecutionResult = await unitOfWork.ScheduledJobExecutionRepository.UpdateAsync(
                    CreateClosedExecution(openExecution, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                if (updateExecutionResult.IsFailure)
                    return ScheduledJobStatus.Added;
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("The interrupted execution of the scheduled job '{ScheduledJobName}' was closed.", scheduledJob.Name);
            return reconciledStatus;
        }
        catch (OperationCanceledException)
        {
            return ScheduledJobStatus.Added;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to reconcile the interrupted execution of the scheduled job '{ScheduledJobId}'.", scheduledJobId.Value);
            return ScheduledJobStatus.Added;
        }
    }

    /// <summary>
    /// Gets the schedule of the scheduled job identified by <paramref name="scheduledJobId"/>, or <see langword="null"/> when it cannot be loaded.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose schedule is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The schedule of the scheduled job, or <see langword="null"/>.</returns>
    private async Task<Schedule?> GetScheduleAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        Result<ScheduledJobEntity?> getScheduledJobResult = await unitOfWork.ScheduledJobRepository.GetByIdAsync(scheduledJobId.Value, cancellationToken).ConfigureAwait(false);
        if (getScheduledJobResult.IsFailure || getScheduledJobResult.Value is null)
            return null;
        Result<ScheduledJob> scheduledJobDomainResult = getScheduledJobResult.Value.ToDomainEntity();
        return scheduledJobDomainResult.IsFailure ? null : scheduledJobDomainResult.Value.Schedule;
    }

    /// <summary>
    /// Calculates the delay until the next execution of the scheduled job, based on its <paramref name="schedule"/>.
    /// </summary>
    /// <param name="schedule">The schedule used to calculate the delay.</param>
    /// <returns>The calculated delay until the next execution of the scheduled job.</returns>
    private async Task<TimeSpan> CalculateDelayAsync(Schedule schedule)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IDateTimeProvider dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        // The daily hour and minute of a schedule are expressed in the local time of the server, so that a daily schedule follows the clock the server runs on.
        return schedule.GetDelayUntilNextExecution(dateTimeProvider.UtcNow, TimeZoneInfo.Local);
    }

    /// <summary>
    /// Creates a copy of <paramref name="scheduledJob"/> with the provided status and timestamps.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job to copy.</param>
    /// <param name="status">The status of the copy.</param>
    /// <param name="lastStartedOnUtc">The last start time of the copy.</param>
    /// <param name="lastCompletedOnUtc">The last completion time of the copy.</param>
    /// <returns>The copy of the scheduled job.</returns>
    private static ScheduledJobEntity CreateUpdatedScheduledJob(ScheduledJobEntity scheduledJob, ScheduledJobStatus status, DateTime? lastStartedOnUtc, DateTime? lastCompletedOnUtc)
    {
        return new ScheduledJobEntity
        {
            Id = scheduledJob.Id,
            Name = scheduledJob.Name,
            TaskType = scheduledJob.TaskType,
            ScheduleType = scheduledJob.ScheduleType,
            IntervalMinutes = scheduledJob.IntervalMinutes,
            Hour = scheduledJob.Hour,
            Minute = scheduledJob.Minute,
            Status = status,
            OwnerUserId = scheduledJob.OwnerUserId,
            LastStartedOnUtc = lastStartedOnUtc,
            LastCompletedOnUtc = lastCompletedOnUtc,
            CreatedOnUtc = scheduledJob.CreatedOnUtc,
            CreatedBy = scheduledJob.CreatedBy,
            UpdatedOnUtc = scheduledJob.UpdatedOnUtc,
            UpdatedBy = scheduledJob.UpdatedBy
        };
    }

    /// <summary>
    /// Creates a copy of <paramref name="execution"/> with the provided completion time.
    /// </summary>
    /// <param name="execution">The execution to copy.</param>
    /// <param name="completedOnUtc">The completion time of the copy.</param>
    /// <returns>The copy of the execution.</returns>
    private static ScheduledJobExecutionEntity CreateClosedExecution(ScheduledJobExecutionEntity execution, DateTime? completedOnUtc)
    {
        return new ScheduledJobExecutionEntity
        {
            Id = execution.Id,
            ScheduledJobId = execution.ScheduledJobId,
            TaskType = execution.TaskType,
            IsCycleRun = execution.IsCycleRun,
            WasCycleActive = execution.WasCycleActive,
            StartedOnUtc = execution.StartedOnUtc,
            CompletedOnUtc = completedOnUtc,
            CreatedOnUtc = execution.CreatedOnUtc,
            CreatedBy = execution.CreatedBy,
            UpdatedOnUtc = execution.UpdatedOnUtc,
            UpdatedBy = execution.UpdatedBy
        };
    }
}
