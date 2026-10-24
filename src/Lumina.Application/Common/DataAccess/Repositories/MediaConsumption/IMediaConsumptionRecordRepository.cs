#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;

/// <summary>
/// Interface for the repository for media consumption records.
/// </summary>
public interface IMediaConsumptionRecordRepository : IRepository<MediaConsumptionRecordEntity>,
                                                     IInsertRepositoryAction<MediaConsumptionRecordEntity>
{
    /// <summary>
    /// Gets the consumption record identified by <paramref name="id"/> when it belongs to the user identified by <paramref name="userId"/>, if it exists.
    /// </summary>
    /// <param name="id">The Id of the consumption record to get.</param>
    /// <param name="userId">The Id of the user the consumption record must belong to.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved record should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the consumption record identified by <paramref name="id"/> and owned by
    /// <paramref name="userId"/>, <see langword="null"/> when no such record exists, or an error.
    /// </returns>
    Task<Result<MediaConsumptionRecordEntity?>> GetByIdForUserAsync(Guid id, Guid userId, bool shouldTrackEntities = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the consumption record of the user identified by <paramref name="userId"/>, when the record belongs to that user and still holds the
    /// version it was read with.
    /// </summary>
    /// <param name="data">The consumption record whose editable values are applied to the stored record. Its owner must match <paramref name="userId"/>.</param>
    /// <param name="userId">The Id of the user the consumption record must belong to. It is authoritative for locating the stored record; the owner carried by <paramref name="data"/> is not used to select it.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    Task<Result<Updated>> UpdateForUserAsync(MediaConsumptionRecordEntity data, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the consumption record identified by <paramref name="id"/> when it belongs to the user identified by <paramref name="userId"/>. A record
    /// that does not exist is not an error.
    /// </summary>
    /// <param name="id">The Id of the consumption record to delete.</param>
    /// <param name="userId">The Id of the user the consumption record must belong to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    Task<Result<Deleted>> DeleteForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes every consumption record of the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose consumption records are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    Task<Result<Deleted>> DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
