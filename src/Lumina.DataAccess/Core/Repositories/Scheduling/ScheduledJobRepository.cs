#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Scheduling;

/// <summary>
/// Repository for scheduled jobs.
/// </summary>
internal sealed class ScheduledJobRepository : IScheduledJobRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public ScheduledJobRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets a scheduled job identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the scheduled job to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="ScheduledJobEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<Result<ScheduledJobEntity?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // FindAsync returns an entity that was already loaded in the current unit of work from the change tracker, and only
        // reads it from the storage medium when it is not tracked, so an entity loaded earlier is not read again.
        return await _luminaDbContext.ScheduledJobs.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a scheduled job identified by <paramref name="id"/> from the storage medium, without tracking it.
    /// </summary>
    /// <param name="id">The id of the scheduled job to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="ScheduledJobEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<Result<ScheduledJobEntity?>> GetByIdWithoutTrackingAsync(Guid id, CancellationToken cancellationToken)
    {
        // The entity is read without tracking, so a concurrent update made by another unit of work is never hidden by an already tracked copy.
        return await _luminaDbContext.ScheduledJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(scheduledJob => scheduledJob.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all scheduled jobs from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="ScheduledJobEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<ScheduledJobEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.ScheduledJobs
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the scheduled jobs that have an active or running execution cycle, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="ScheduledJobEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<ScheduledJobEntity>>> GetActiveOrRunningAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.ScheduledJobs
            .Where(scheduledJob => scheduledJob.Status == ScheduledJobStatus.Active || scheduledJob.Status == ScheduledJobStatus.Running)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a new scheduled job.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(ScheduledJobEntity scheduledJob, CancellationToken cancellationToken)
    {
        bool doesScheduledJobExist = await _luminaDbContext.ScheduledJobs.AnyAsync(
            existingScheduledJob => existingScheduledJob.Id == scheduledJob.Id, cancellationToken).ConfigureAwait(false);
        if (doesScheduledJobExist)
            return Errors.Scheduling.ScheduledJobAlreadyExists;

        _luminaDbContext.ScheduledJobs.Add(scheduledJob);
        return Result.Created;
    }

    /// <summary>
    /// Updates a scheduled job.
    /// </summary>
    /// <param name="data">The scheduled job to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(ScheduledJobEntity data, CancellationToken cancellationToken)
    {
        ScheduledJobEntity? foundScheduledJob = _luminaDbContext.ScheduledJobs.Local.FirstOrDefault(scheduledJob => scheduledJob.Id == data.Id)
            ?? await _luminaDbContext.ScheduledJobs.FirstOrDefaultAsync(scheduledJob => scheduledJob.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundScheduledJob is null)
            return Errors.Scheduling.ScheduledJobNotFound;
        // Update scalar properties.
        _luminaDbContext.Entry(foundScheduledJob).CurrentValues.SetValues(data);
        return Result.Updated;
    }

    /// <summary>
    /// Removes a scheduled job identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the scheduled job to remove.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ScheduledJobEntity? foundScheduledJob = await _luminaDbContext.ScheduledJobs
            .FirstOrDefaultAsync(scheduledJob => scheduledJob.Id == id, cancellationToken).ConfigureAwait(false);
        if (foundScheduledJob is null)
            return Errors.Scheduling.ScheduledJobNotFound;

        _luminaDbContext.ScheduledJobs.Remove(foundScheduledJob);
        return Result.Deleted;
    }
}
