#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Scheduling;

/// <summary>
/// Interface for the repository for scheduled jobs.
/// </summary>
public interface IScheduledJobRepository : IRepository<ScheduledJobEntity>,
                                           IGetByIdRepositoryAction<ScheduledJobEntity, Guid>,
                                           IGetAllRepositoryAction<ScheduledJobEntity>,
                                           IInsertRepositoryAction<ScheduledJobEntity>,
                                           IUpdateRepositoryAction<ScheduledJobEntity>,
                                           IDeleteByIdRepositoryAction<Guid>
{
    /// <summary>
    /// Gets a scheduled job identified by <paramref name="id"/> from the storage medium, without tracking it.
    /// </summary>
    /// <param name="id">The id of the scheduled job to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="ScheduledJobEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    Task<Result<ScheduledJobEntity?>> GetByIdWithoutTrackingAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the scheduled jobs that have an active or running execution cycle, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="ScheduledJobEntity"/>, or an error.</returns>
    Task<Result<IEnumerable<ScheduledJobEntity>>> GetActiveOrRunningAsync(CancellationToken cancellationToken);
}
