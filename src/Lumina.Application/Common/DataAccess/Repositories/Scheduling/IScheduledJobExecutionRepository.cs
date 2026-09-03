#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Scheduling;

/// <summary>
/// Interface for the repository for the executions of the tasks of scheduled jobs.
/// </summary>
public interface IScheduledJobExecutionRepository : IRepository<ScheduledJobExecutionEntity>,
                                                    IGetByIdRepositoryAction<ScheduledJobExecutionEntity, Guid>,
                                                    IInsertRepositoryAction<ScheduledJobExecutionEntity>,
                                                    IUpdateRepositoryAction<ScheduledJobExecutionEntity>
{
    /// <summary>
    /// Gets the most recent execution of the task of a scheduled job identified by <paramref name="scheduledJobId"/> that is still running, from the storage medium.
    /// </summary>
    /// <param name="scheduledJobId">The Id of the scheduled job whose running execution is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the running execution of the scheduled job, or <see langword="null"/>.</returns>
    Task<Result<ScheduledJobExecutionEntity?>> GetOpenByScheduledJobIdAsync(Guid scheduledJobId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the executions of the tasks of scheduled jobs that started in the interval between <paramref name="fromUtc"/> and <paramref name="toUtc"/>, from the storage medium.
    /// </summary>
    /// <param name="fromUtc">The inclusive lower bound of the interval, in UTC.</param>
    /// <param name="toUtc">The inclusive upper bound of the interval, in UTC.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="ScheduledJobExecutionEntity"/>, or an error.</returns>
    Task<Result<IEnumerable<ScheduledJobExecutionEntity>>> GetByTimeRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Removes all the executions of the task of a scheduled job identified by <paramref name="scheduledJobId"/>, from the storage medium.
    /// </summary>
    /// <param name="scheduledJobId">The Id of the scheduled job whose executions are removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> DeleteByScheduledJobIdAsync(Guid scheduledJobId, CancellationToken cancellationToken);
}
