#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MediaConsumption;

/// <summary>
/// Repository for the consumption records used to build consumption statistics.
/// </summary>
/// <remarks>
/// The repository only binds the filters and returns the records. Counting, grouping, ordering, display data resolution, and mapping stay in the
/// application handlers, so the same records can serve the summary, the history lists, the all-users summary, and the tag breakdown.
/// </remarks>
internal sealed class ConsumptionStatisticsRepository : IConsumptionStatisticsRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumptionStatisticsRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public ConsumptionStatisticsRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets the consumption records of the user identified by <paramref name="userId"/>, optionally limited to a media library, a media kind, and a date range.
    /// </summary>
    /// <param name="userId">The Id of the user whose consumption records are retrieved.</param>
    /// <param name="libraryId">The Id of the media library to restrict the records to, or <see langword="null"/> for every library.</param>
    /// <param name="mediaKind">The kind of media to restrict the records to, or <see langword="null"/> for every kind.</param>
    /// <param name="fromUtc">The start of the date range, or <see langword="null"/> when the range has no start.</param>
    /// <param name="toUtc">The end of the date range, or <see langword="null"/> when the range has no end.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the consumption records of the user, or an error.</returns>
    public async Task<Result<IReadOnlyList<MediaConsumptionRecordEntity>>> GetForUserAsync(
        Guid userId, Guid? libraryId, MediaKind? mediaKind, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken)
    {
        // The user is part of the query rather than a check performed after the read, so the storage medium never returns a record of another user.
        IQueryable<MediaConsumptionRecordEntity> query = _luminaDbContext.MediaConsumptionRecords
            .AsNoTracking()
            .Where(record => record.UserId == userId);
        query = ApplyFilters(query, libraryId, mediaKind, fromUtc, toUtc);
        List<MediaConsumptionRecordEntity> records = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>(records);
    }

    /// <summary>
    /// Gets the consumption records of every user, optionally limited to a media library, a media kind, and a date range.
    /// </summary>
    /// <param name="libraryId">The Id of the media library to restrict the records to, or <see langword="null"/> for every library.</param>
    /// <param name="mediaKind">The kind of media to restrict the records to, or <see langword="null"/> for every kind.</param>
    /// <param name="fromUtc">The start of the date range, or <see langword="null"/> when the range has no start.</param>
    /// <param name="toUtc">The end of the date range, or <see langword="null"/> when the range has no end.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the consumption records of every user, or an error.</returns>
    public async Task<Result<IReadOnlyList<MediaConsumptionRecordEntity>>> GetForAllUsersAsync(
        Guid? libraryId, MediaKind? mediaKind, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken)
    {
        // This is the only query that is not scoped to one user, and it exists only for the explicit admin all-users summary.
        IQueryable<MediaConsumptionRecordEntity> query = _luminaDbContext.MediaConsumptionRecords.AsNoTracking();
        query = ApplyFilters(query, libraryId, mediaKind, fromUtc, toUtc);
        List<MediaConsumptionRecordEntity> records = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>(records);
    }

    /// <summary>
    /// Applies the optional library, media kind, and date range filters to <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The query to filter.</param>
    /// <param name="libraryId">The Id of the media library to restrict the records to, or <see langword="null"/> for every library.</param>
    /// <param name="mediaKind">The kind of media to restrict the records to, or <see langword="null"/> for every kind.</param>
    /// <param name="fromUtc">The start of the date range, or <see langword="null"/> when the range has no start.</param>
    /// <param name="toUtc">The end of the date range, or <see langword="null"/> when the range has no end.</param>
    /// <returns>The filtered query.</returns>
    private static IQueryable<MediaConsumptionRecordEntity> ApplyFilters(
        IQueryable<MediaConsumptionRecordEntity> query, Guid? libraryId, MediaKind? mediaKind, DateTime? fromUtc, DateTime? toUtc)
    {
        // Every filter is bound in the query, so the database never materializes rows that the caller did not ask for.
        if (libraryId.HasValue)
            query = query.Where(record => record.LibraryId == libraryId.Value);
        if (mediaKind.HasValue)
            query = query.Where(record => record.MediaKind == mediaKind.Value);
        // The date range is applied to the start of the session, which is the indexed timestamp the statistics of a user are read by.
        if (fromUtc.HasValue)
            query = query.Where(record => record.StartedOnUtc >= fromUtc.Value);
        if (toUtc.HasValue)
            query = query.Where(record => record.StartedOnUtc <= toUtc.Value);
        return query;
    }
}
