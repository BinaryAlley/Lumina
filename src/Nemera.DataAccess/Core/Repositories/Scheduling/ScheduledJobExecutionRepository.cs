#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Common.DataAccess.Repositories.Scheduling;
using Nemera.DataAccess.Common.Persistence;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.Core.Repositories.Scheduling;

/// <summary>
/// Repository for the executions of the tasks of scheduled jobs.
/// </summary>
internal sealed class ScheduledJobExecutionRepository : IScheduledJobExecutionRepository
{
    private readonly NemeraDbContext _nemeraDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionRepository"/> class.
    /// </summary>
    /// <param name="nemeraDbContext">Injected Entity Framework DbContext.</param>
    public ScheduledJobExecutionRepository(NemeraDbContext nemeraDbContext)
    {
        _nemeraDbContext = nemeraDbContext;
    }

    /// <summary>
    /// Gets an execution identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the execution to get.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the execution should be loaded together with the execution itself.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved execution should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="ScheduledJobExecutionEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<Result<ScheduledJobExecutionEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        // The execution is read with a query instead of FindAsync so that it can be retrieved without tracking when requested.
        // When the entity is tracked, the change tracker still returns the already loaded instance instead of a new one.
        IQueryable<ScheduledJobExecutionEntity> query = _nemeraDbContext.ScheduledJobExecutions;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(execution => execution.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a new execution of the task of a scheduled job.
    /// </summary>
    /// <param name="execution">The execution to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(ScheduledJobExecutionEntity execution, CancellationToken cancellationToken)
    {
        bool doesExecutionExist = await _nemeraDbContext.ScheduledJobExecutions.AnyAsync(
            existingExecution => existingExecution.Id == execution.Id, cancellationToken).ConfigureAwait(false);
        if (doesExecutionExist)
            return Errors.Scheduling.ScheduledJobExecutionAlreadyExists;

        _nemeraDbContext.ScheduledJobExecutions.Add(execution);
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
        ScheduledJobExecutionEntity? foundExecution = _nemeraDbContext.ScheduledJobExecutions.Local.FirstOrDefault(execution => execution.Id == data.Id)
            ?? await _nemeraDbContext.ScheduledJobExecutions.FirstOrDefaultAsync(execution => execution.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundExecution is null)
            return Errors.Scheduling.ScheduledJobExecutionNotFound;
        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        EditableValuesCopier.CopyEditableValues(_nemeraDbContext, foundExecution, data);
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
        return await _nemeraDbContext.ScheduledJobExecutions
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
        return await _nemeraDbContext.ScheduledJobExecutions
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
        ScheduledJobExecutionEntity[] executions = await _nemeraDbContext.ScheduledJobExecutions
            .Where(execution => execution.ScheduledJobId == scheduledJobId)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        _nemeraDbContext.ScheduledJobExecutions.RemoveRange(executions);
        return Result.Success;
    }

    /// <summary>
    /// Removes all the executions of the tasks of scheduled jobs that started before <paramref name="cutoffUtc"/>, from the storage medium.
    /// </summary>
    /// <param name="cutoffUtc">The exclusive lower bound of the retained interval, in UTC; executions that started before it are removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken)
    {
        // The executions are deleted with a single query, without loading them into memory first, because the retention cleanup
        // routinely removes a large number of executions at once.
        await _nemeraDbContext.ScheduledJobExecutions
            .Where(execution => execution.StartedOnUtc < cutoffUtc)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success;
    }
}
