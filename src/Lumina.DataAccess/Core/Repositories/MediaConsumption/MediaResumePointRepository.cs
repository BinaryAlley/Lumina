#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.DataAccess.Common.Persistence;
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
/// Repository for media resume points.
/// </summary>
/// <remarks>
/// The business key of a resume point is the user, the media kind, and the media item. The unique index on those columns makes the storage
/// medium the owner of the "one resume point per user and item" invariant, and an upsert updates the existing row instead of inserting a second one.
/// </remarks>
internal sealed class MediaResumePointRepository : IMediaResumePointRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaResumePointRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public MediaResumePointRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets the resume point of the user identified by <paramref name="userId"/> for the media item identified by <paramref name="mediaKind"/> and <paramref name="mediaItemId"/>, if it exists.
    /// </summary>
    /// <param name="userId">The Id of the user the resume point must belong to.</param>
    /// <param name="mediaKind">The kind of the media item to resume.</param>
    /// <param name="mediaItemId">The Id of the media item to resume.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved resume point should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the resume point of the user for the media item, <see langword="null"/> when no such resume point exists, or an error.
    /// </returns>
    public async Task<Result<MediaResumePointEntity?>> GetForUserAndMediaItemAsync(Guid userId, MediaKind mediaKind, Guid mediaItemId, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        // The user is part of the filter rather than a check performed after the read, so the storage medium never returns a resume point that belongs to another user in the first place.
        IQueryable<MediaResumePointEntity> query = _luminaDbContext.MediaResumePoints
            .Where(resumePoint => resumePoint.UserId == userId &&
                                  resumePoint.MediaKind == mediaKind &&
                                  resumePoint.MediaItemId == mediaItemId);
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Saves the resume point of the user identified by <paramref name="userId"/>, inserting it when it does not exist and updating it otherwise. The
    /// business key of the resume point, made of the user, the media kind, and the media item, is preserved.
    /// </summary>
    /// <param name="data">The resume point whose values are saved. Its owner must match <paramref name="userId"/>.</param>
    /// <param name="userId">The Id of the user the resume point must belong to. It is authoritative for locating the stored resume point and for the owner of a new one; the owner carried by <paramref name="data"/> is not used to select it.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful save, or an error.</returns>
    public async Task<Result<Updated>> UpsertForUserAsync(MediaResumePointEntity data, Guid userId, CancellationToken cancellationToken)
    {
        // The business key is located in the tracked entities first, so an upsert of a resume point that this unit of work already loaded updates that instance instead of materializing a second copy.
        MediaResumePointEntity? foundResumePoint = _luminaDbContext.MediaResumePoints.Local
            .FirstOrDefault(resumePoint => resumePoint.UserId == userId &&
                                           resumePoint.MediaKind == data.MediaKind &&
                                           resumePoint.MediaItemId == data.MediaItemId)
            ?? await _luminaDbContext.MediaResumePoints
                .FirstOrDefaultAsync(resumePoint => resumePoint.UserId == userId &&
                                                   resumePoint.MediaKind == data.MediaKind &&
                                                   resumePoint.MediaItemId == data.MediaItemId, cancellationToken).ConfigureAwait(false);
        if (foundResumePoint is null)
        {
            // The user argument is authoritative for the owner of the new row, not the owner carried by the caller.
            data.UserId = userId;
            _luminaDbContext.MediaResumePoints.Add(data);
            return Result.Updated;
        }

        // A report that carries the values that are already stored is a no-op: writing it would bump the version for nothing.
        if (!HasEditableChanges(foundResumePoint, data))
            return Result.Updated;

        // The business key is never reassigned by an upsert. The version is captured before the copy and incremented after it, so the update is only applied when the row still holds the version the report read.
        Guid storedUserId = foundResumePoint.UserId;
        MediaKind mediaKind = foundResumePoint.MediaKind;
        Guid mediaItemId = foundResumePoint.MediaItemId;
        long version = foundResumePoint.Version;
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundResumePoint, data);
        foundResumePoint.UserId = storedUserId;
        foundResumePoint.MediaKind = mediaKind;
        foundResumePoint.MediaItemId = mediaItemId;
        foundResumePoint.Version = version + 1;
        return Result.Updated;
    }

    /// <summary>
    /// Deletes the resume point of the user identified by <paramref name="userId"/> for the media item identified by <paramref name="mediaKind"/> and
    /// <paramref name="mediaItemId"/>. A resume point that does not exist is not an error.
    /// </summary>
    /// <param name="userId">The Id of the user the resume point must belong to.</param>
    /// <param name="mediaKind">The kind of the media item to resume.</param>
    /// <param name="mediaItemId">The Id of the media item to resume.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    public async Task<Result<Deleted>> DeleteForUserAndMediaItemAsync(Guid userId, MediaKind mediaKind, Guid mediaItemId, CancellationToken cancellationToken)
    {
        MediaResumePointEntity? resumePoint = _luminaDbContext.MediaResumePoints.Local
            .FirstOrDefault(trackedResumePoint => trackedResumePoint.UserId == userId &&
                                                  trackedResumePoint.MediaKind == mediaKind &&
                                                  trackedResumePoint.MediaItemId == mediaItemId)
            ?? await _luminaDbContext.MediaResumePoints
                .FirstOrDefaultAsync(foundResumePoint => foundResumePoint.UserId == userId &&
                                                         foundResumePoint.MediaKind == mediaKind &&
                                                         foundResumePoint.MediaItemId == mediaItemId, cancellationToken).ConfigureAwait(false);
        if (resumePoint is null)
            return Result.Deleted;

        _luminaDbContext.MediaResumePoints.Remove(resumePoint);
        return Result.Deleted;
    }

    /// <summary>
    /// Deletes every resume point of the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose resume points are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    public async Task<Result<Deleted>> DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<MediaResumePointEntity> resumePoints = await _luminaDbContext.MediaResumePoints
            .Where(resumePoint => resumePoint.UserId == userId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (resumePoints.Count > 0)
            _luminaDbContext.MediaResumePoints.RemoveRange(resumePoints);
        return Result.Deleted;
    }

    /// <summary>
    /// Deletes every resume point of the user identified by <paramref name="userId"/> in the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose resume points are deleted.</param>
    /// <param name="libraryId">The Id of the media library whose resume points are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    public async Task<Result<Deleted>> DeleteAllForUserInLibraryAsync(Guid userId, Guid libraryId, CancellationToken cancellationToken)
    {
        List<MediaResumePointEntity> resumePoints = await _luminaDbContext.MediaResumePoints
            .Where(resumePoint => resumePoint.UserId == userId && resumePoint.LibraryId == libraryId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (resumePoints.Count > 0)
            _luminaDbContext.MediaResumePoints.RemoveRange(resumePoints);
        return Result.Deleted;
    }

    /// <summary>
    /// Determines whether <paramref name="incoming"/> carries values that differ from the stored <paramref name="stored"/> resume point.
    /// </summary>
    /// <param name="stored">The resume point currently stored in the persistence medium.</param>
    /// <param name="incoming">The resume point carrying the values to apply.</param>
    /// <returns><see langword="true"/> if an editable value changed, <see langword="false"/> otherwise.</returns>
    private static bool HasEditableChanges(MediaResumePointEntity stored, MediaResumePointEntity incoming)
    {
        return stored.LibraryId != incoming.LibraryId ||
               stored.LastPositionKind != incoming.LastPositionKind ||
               stored.LastPositionTimeOffsetMilliseconds != incoming.LastPositionTimeOffsetMilliseconds ||
               stored.LastPositionLocation != incoming.LastPositionLocation ||
               stored.LastProgressFraction != incoming.LastProgressFraction ||
               stored.LastConsumedOnUtc != incoming.LastConsumedOnUtc;
    }
}
