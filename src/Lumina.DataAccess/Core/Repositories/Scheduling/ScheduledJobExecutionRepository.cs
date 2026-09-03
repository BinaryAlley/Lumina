#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Scheduling;

/// <summary>
/// Repository for the executions of the tasks of scheduled jobs.
/// </summary>
internal sealed class ScheduledJobExecutionRepository : IScheduledJobExecutionRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public ScheduledJobExecutionRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets an execution identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the execution to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="ScheduledJobExecutionEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<Result<ScheduledJobExecutionEntity?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.ScheduledJobExecutions
            .FirstOrDefaultAsync(execution => execution.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a new execution of the task of a scheduled job.
    /// </summary>
    /// <param name="execution">The execution to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(ScheduledJobExecutionEntity execution, CancellationToken cancellationToken)
    {
        bool doesExecutionExist = await _luminaDbContext.ScheduledJobExecutions.AnyAsync(
            existingExecution => existingExecution.Id == execution.Id, cancellationToken).ConfigureAwait(false);
        if (doesExecutionExist)
            return Errors.Scheduling.ScheduledJobExecutionAlreadyExists;

        _luminaDbContext.ScheduledJobExecutions.Add(execution);
        return Result.Created;
    }

    /// <summary>
    /// Updates an execution of the task of a scheduled job.
    /// </summary>
    /// <param name="data">The execution to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(ScheduledJobExecutionEntity data, CancellationToken cancellationToken)
    {
        ScheduledJobExecutionEntity? foundExecution = await _luminaDbContext.ScheduledJobExecutions
            .FirstOrDefaultAsync(execution => execution.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundExecution is null)
            return Errors.Scheduling.ScheduledJobExecutionNotFound;
        // Update scalar properties.
        _luminaDbContext.Entry(foundExecution).CurrentValues.SetValues(data);
        return Result.Updated;
    }

    /// <summary>
    /// Gets the most recent execution of the task of a scheduled job identified by <paramref name="scheduledJobId"/> that is still running, from the storage medium.
    /// </summary>
    /// <param name="scheduledJobId">The Id of the scheduled job whose running execution is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the running execution of the scheduled job, or <see langword="null"/>.</returns>
    public async Task<Result<ScheduledJobExecutionEntity?>> GetOpenByScheduledJobIdAsync(Guid scheduledJobId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.ScheduledJobExecutions
            .Where(execution => execution.ScheduledJobId == scheduledJobId && execution.CompletedOnUtc == null)
            .OrderByDescending(execution => execution.StartedOnUtc)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the executions of the tasks of scheduled jobs that started in the interval between <paramref name="fromUtc"/> and <paramref name="toUtc"/>, from the storage medium.
    /// </summary>
    /// <param name="fromUtc">The inclusive lower bound of the interval, in UTC.</param>
    /// <param name="toUtc">The inclusive upper bound of the interval, in UTC.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="ScheduledJobExecutionEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<ScheduledJobExecutionEntity>>> GetByTimeRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.ScheduledJobExecutions
            .Where(execution => execution.StartedOnUtc >= fromUtc && execution.StartedOnUtc <= toUtc)
            .OrderBy(execution => execution.StartedOnUtc)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes all the executions of the task of a scheduled job identified by <paramref name="scheduledJobId"/>, from the storage medium.
    /// </summary>
    /// <param name="scheduledJobId">The Id of the scheduled job whose executions are removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> DeleteByScheduledJobIdAsync(Guid scheduledJobId, CancellationToken cancellationToken)
    {
        ScheduledJobExecutionEntity[] executions = await _luminaDbContext.ScheduledJobExecutions
            .Where(execution => execution.ScheduledJobId == scheduledJobId)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        _luminaDbContext.ScheduledJobExecutions.RemoveRange(executions);
        return Result.Success;
    }
}
