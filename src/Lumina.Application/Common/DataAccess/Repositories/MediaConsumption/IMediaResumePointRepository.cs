#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;

/// <summary>
/// Interface for the repository for media resume points.
/// </summary>
public interface IMediaResumePointRepository : IRepository<MediaResumePointEntity>
{
    /// <summary>
    /// Gets the resume point of the user identified by <paramref name="userId"/> for the media item identified by <paramref name="mediaKind"/> and
    /// <paramref name="mediaItemId"/>, if it exists.
    /// </summary>
    /// <param name="userId">The Id of the user the resume point must belong to.</param>
    /// <param name="mediaKind">The kind of the media item to resume.</param>
    /// <param name="mediaItemId">The Id of the media item to resume.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved resume point should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the resume point of the user for the media item, <see langword="null"/> when no such resume
    /// point exists, or an error.
    /// </returns>
    Task<Result<MediaResumePointEntity?>> GetForUserAndMediaItemAsync(Guid userId, MediaKind mediaKind, Guid mediaItemId, bool shouldTrackEntities = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the resume point of the user identified by <paramref name="userId"/>, inserting it when it does not exist and updating it otherwise. The
    /// business key of the resume point, made of the user, the media kind, and the media item, is preserved.
    /// </summary>
    /// <param name="data">The resume point whose values are saved. Its owner must match <paramref name="userId"/>.</param>
    /// <param name="userId">The Id of the user the resume point must belong to. It is authoritative for locating the stored resume point and for the owner of a new one; the owner carried by <paramref name="data"/> is not used to select it.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful save, or an error.</returns>
    Task<Result<Updated>> UpsertForUserAsync(MediaResumePointEntity data, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the resume point of the user identified by <paramref name="userId"/> for the media item identified by <paramref name="mediaKind"/> and
    /// <paramref name="mediaItemId"/>. A resume point that does not exist is not an error.
    /// </summary>
    /// <param name="userId">The Id of the user the resume point must belong to.</param>
    /// <param name="mediaKind">The kind of the media item to resume.</param>
    /// <param name="mediaItemId">The Id of the media item to resume.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    Task<Result<Deleted>> DeleteForUserAndMediaItemAsync(Guid userId, MediaKind mediaKind, Guid mediaItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes every resume point of the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose resume points are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    Task<Result<Deleted>> DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes every resume point of the user identified by <paramref name="userId"/> in the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose resume points are deleted.</param>
    /// <param name="libraryId">The Id of the media library whose resume points are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    Task<Result<Deleted>> DeleteAllForUserInLibraryAsync(Guid userId, Guid libraryId, CancellationToken cancellationToken);
}
