#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
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
    public async Task<Result<ScheduledJobEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        // The scheduled job is read with a query instead of FindAsync so that it can be retrieved without tracking when requested.
        // When the entity is tracked, the change tracker still returns the already loaded instance instead of a new one.
        IQueryable<ScheduledJobEntity> query = _luminaDbContext.ScheduledJobs;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(scheduledJob => scheduledJob.Id == id, cancellationToken).ConfigureAwait(false);
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
    /// Gets all the scheduled jobs from the storage medium, or a page of them when the pagination data is provided.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all the matching scheduled jobs are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="ScheduledJobEntity"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<ScheduledJobEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<ScheduledJobEntity> query = _luminaDbContext.ScheduledJobs;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();

        // If no pagination was requested, return all the scheduled jobs.
        if (paginationData is null)
        {
            IReadOnlyList<ScheduledJobEntity> allScheduledJobs = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<ScheduledJobEntity>
            {
                Data = allScheduledJobs,
                CurrentPage = 1,
                PerPage = allScheduledJobs.Count,
                Count = allScheduledJobs.Count,
                NumberOfPages = 1
            };
        }

        int count = await query.Select(scheduledJob => scheduledJob.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        IReadOnlyList<ScheduledJobEntity> paginatedResult = await query
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<ScheduledJobEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
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
