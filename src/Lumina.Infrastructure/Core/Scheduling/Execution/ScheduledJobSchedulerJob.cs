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
            RunCycleWorkerAsync(ScheduledJobId.Create(scheduledJob.Id), runImmediately: false, cancellationToken).FireAndForgetSafeAsync();
    }

    /// <summary>
    /// Runs the execution cycle worker of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is run.</param>
    /// <param name="runImmediately">Whether the task of the scheduled job is run immediately when the cycle starts.</param>
    /// <param name="stoppingToken">Cancellation token that can be used to stop the execution.</param>
    private async Task RunCycleWorkerAsync(ScheduledJobId scheduledJobId, bool runImmediately, CancellationToken stoppingToken)
    {
        CancellationTokenSource cycleCancellationTokenSource = new();
        if (!_runtimeRegistry.TryStartCycle(scheduledJobId, cycleCancellationTokenSource))
        {
            cycleCancellationTokenSource.Dispose();
            return;
        }

        try
        {
            CancellationToken cycleToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cycleCancellationTokenSource.Token).Token;
            if (runImmediately)
                await RunScheduledJobAsync(scheduledJobId, isCycleRun: true, cycleToken).ConfigureAwait(false);

            Schedule? schedule = await GetScheduleAsync(scheduledJobId, cycleToken).ConfigureAwait(false);
            if (schedule is null)
                return;

            while (true)
            {
                TimeSpan delay = await CalculateDelayAsync(schedule).ConfigureAwait(false);
                using PeriodicTimer timer = new(delay);
                while (await timer.WaitForNextTickAsync(cycleToken).ConfigureAwait(false))
                {
                    if (cycleToken.IsCancellationRequested)
                        return;
                    await RunScheduledJobAsync(scheduledJobId, isCycleRun: true, cycleToken).ConfigureAwait(false);
                    // A daily schedule needs its delay recalculated after every run, so the next run happens at the next day's hour and minute.
                    if (schedule.ScheduleType == ScheduleType.DailyAtHourAndMinute)
                        timer.Period = await CalculateDelayAsync(schedule).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The execution cycle of the scheduled job '{ScheduledJobId}' failed.", scheduledJobId.Value);
        }
        finally
        {
            _runtimeRegistry.StopCycle(scheduledJobId);
            cycleCancellationTokenSource.Dispose();
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
        Result<Success> startExecutionResult = scheduledJobDomainResult.Value.MarkExecutionStarted(isCycleRun);
        if (startExecutionResult.IsFailure)
            return;
        foreach (IDomainEvent domainEvent in scheduledJobDomainResult.Value.GetDomainEvents())
            await domainEventPublisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);

        // Execute the payload of the task of the scheduled job.
        IScheduledTaskExecutorFactory taskExecutorFactory = services.GetRequiredService<IScheduledTaskExecutorFactory>();
        IScheduledTaskExecutor taskExecutor = taskExecutorFactory.CreateExecutor(scheduledJobDomainResult.Value.TaskType);
        Result<Success> executePayloadResult = await taskExecutor.ExecutePayloadAsync(scheduledJobDomainResult.Value, cancellationToken).ConfigureAwait(false);
        if (executePayloadResult.IsFailure)
            _logger.LogWarning("The task of the scheduled job '{ScheduledJobName}' failed: {Error}", scheduledJobDomainResult.Value.Name, executePayloadResult.FirstError.Description);

        // Mark the task as completed. The same domain entity is reused, because it alone still tracks the run id of the
        // current execution, which is required to raise the completion domain event; a fresh copy reloaded from the
        // storage medium would no longer know which execution to complete.
        Result<Success> completeExecutionResult = scheduledJobDomainResult.Value.MarkExecutionCompleted(isCycleRun);
        if (completeExecutionResult.IsFailure)
            return;
        foreach (IDomainEvent domainEvent in scheduledJobDomainResult.Value.GetDomainEvents())
            await domainEventPublisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
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
}
