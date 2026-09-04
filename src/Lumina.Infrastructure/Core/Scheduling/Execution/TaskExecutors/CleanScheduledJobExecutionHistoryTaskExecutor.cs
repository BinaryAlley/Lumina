#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Task executor that cleans the execution history of the scheduled jobs, keeping only the executions of the past month.
/// </summary>
public class CleanScheduledJobExecutionHistoryTaskExecutor : IScheduledTaskExecutor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CleanScheduledJobExecutionHistoryTaskExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanScheduledJobExecutionHistoryTaskExecutor"/> class.
    /// </summary>
    /// <param name="dateTimeProvider">Injected service for time related concerns.</param>
    /// <param name="logger">Injected service for logging.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public CleanScheduledJobExecutionHistoryTaskExecutor(IDateTimeProvider dateTimeProvider, ILogger<CleanScheduledJobExecutionHistoryTaskExecutor> logger, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Removes the executions of the tasks of the scheduled jobs that are older than a month, keeping only the recent executions for audit.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job whose task is executed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> ExecutePayloadAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        DateTime cutoffUtc = _dateTimeProvider.UtcNow.AddMonths(-1);
        Result<Success> deleteResult = await _unitOfWork.ScheduledJobExecutionRepository.DeleteOlderThanAsync(cutoffUtc, cancellationToken).ConfigureAwait(false);
        if (deleteResult.IsFailure)
            return deleteResult.Errors;

        _logger.LogInformation("Cleaned the execution history of the scheduled jobs on behalf of the scheduled job '{ScheduledJobName}'.", scheduledJob.Name);
        return Result.Success;
    }
}
