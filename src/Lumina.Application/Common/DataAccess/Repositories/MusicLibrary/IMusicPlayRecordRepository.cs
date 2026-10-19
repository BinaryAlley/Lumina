#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;

/// <summary>
/// Interface for the repository for recorded music plays.
/// </summary>
public interface IMusicPlayRecordRepository : IRepository<MusicPlayRecordEntity>,
                                               IInsertRepositoryAction<MusicPlayRecordEntity>
{
    /// <summary>
    /// Gets the play record identified by <paramref name="id"/> when it belongs to the user identified by <paramref name="userId"/>, if it exists.
    /// </summary>
    /// <param name="id">The Id of the play record to get.</param>
    /// <param name="userId">The Id of the user the play record must belong to.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved record should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the play record identified by <paramref name="id"/> and owned by <paramref name="userId"/>,
    /// <see langword="null"/> when no such record exists, or an error.
    /// </returns>
    Task<Result<MusicPlayRecordEntity?>> GetByIdForUserAsync(Guid id, Guid userId, bool shouldTrackEntities = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the play record of the user identified by <paramref name="userId"/>, when the record belongs to that user and still holds the
    /// version it was read with.
    /// </summary>
    /// <param name="data">The play record whose editable values are applied to the stored record.</param>
    /// <param name="userId">The Id of the user the play record must belong to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    Task<Result<Updated>> UpdateForUserAsync(MusicPlayRecordEntity data, Guid userId, CancellationToken cancellationToken);
}
