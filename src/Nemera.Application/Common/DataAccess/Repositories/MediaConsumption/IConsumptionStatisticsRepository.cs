#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;

/// <summary>
/// Interface for the repository for the consumption records used to build consumption statistics.
/// </summary>
public interface IConsumptionStatisticsRepository : IRepository<MediaConsumptionRecordEntity>
{
    /// <summary>
    /// Gets the consumption records of the user identified by <paramref name="userId"/>, optionally limited to a media library, a media kind, and a date
    /// range.
    /// </summary>
    /// <param name="userId">The Id of the user whose consumption records are retrieved.</param>
    /// <param name="libraryId">The Id of the media library to restrict the records to, or <see langword="null"/> for every library.</param>
    /// <param name="mediaKind">The kind of media to restrict the records to, or <see langword="null"/> for every kind.</param>
    /// <param name="fromUtc">The start of the date range, or <see langword="null"/> when the range has no start.</param>
    /// <param name="toUtc">The end of the date range, or <see langword="null"/> when the range has no end.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the consumption records of the user, or an error.</returns>
    Task<Result<IReadOnlyList<MediaConsumptionRecordEntity>>> GetForUserAsync(Guid userId, Guid? libraryId, MediaKind? mediaKind, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the consumption records of every user, optionally limited to a media library, a media kind, and a date range.
    /// </summary>
    /// <param name="libraryId">The Id of the media library to restrict the records to, or <see langword="null"/> for every library.</param>
    /// <param name="mediaKind">The kind of media to restrict the records to, or <see langword="null"/> for every kind.</param>
    /// <param name="fromUtc">The start of the date range, or <see langword="null"/> when the range has no start.</param>
    /// <param name="toUtc">The end of the date range, or <see langword="null"/> when the range has no end.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the consumption records of every user, or an error.</returns>
    Task<Result<IReadOnlyList<MediaConsumptionRecordEntity>>> GetForAllUsersAsync(Guid? libraryId, MediaKind? mediaKind, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken);
}
