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
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MediaConsumption;

/// <summary>
/// Repository for media consumption records.
/// </summary>
/// <remarks>
/// Records are always read, updated, and deleted scoped to their owner. The media item they reference is kept by kind and Id only, so a deleted media item leaves the record and its statistics intact.
/// </remarks>
internal sealed class MediaConsumptionRecordRepository : IMediaConsumptionRecordRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaConsumptionRecordRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public MediaConsumptionRecordRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new consumption record.
    /// </summary>
    /// <param name="data">The consumption record to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Task<Result<Created>> InsertAsync(MediaConsumptionRecordEntity data, CancellationToken cancellationToken)
    {
        // The handler creates a new session with a newly generated Id, so this always adds a row.
        _luminaDbContext.MediaConsumptionRecords.Add(data);
        return Task.FromResult<Result<Created>>(Result.Created);
    }

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
    public async Task<Result<MediaConsumptionRecordEntity?>> GetByIdForUserAsync(Guid id, Guid userId, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        // The user is part of the filter rather than a check performed after the read, so the storage medium never returns a record that belongs to another user in the first place.
        IQueryable<MediaConsumptionRecordEntity> query = _luminaDbContext.MediaConsumptionRecords
            .Where(record => record.Id == id && record.UserId == userId);
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the consumption record of the user identified by <paramref name="userId"/>, when the record belongs to that user and still holds the version it was read with.
    /// </summary>
    /// <param name="data">The consumption record whose editable values are applied to the stored record. Its owner must match <paramref name="userId"/>.</param>
    /// <param name="userId">The Id of the user the consumption record must belong to. It is authoritative for locating the stored record; the owner carried by <paramref name="data"/> is not used to select it.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public async Task<Result<Updated>> UpdateForUserAsync(MediaConsumptionRecordEntity data, Guid userId, CancellationToken cancellationToken)
    {
        // The user is part of both the tracked and the stored lookup, so a caller can never update a record that belongs to another user merely by knowing its Id.
        MediaConsumptionRecordEntity? foundRecord = _luminaDbContext.MediaConsumptionRecords.Local
            .FirstOrDefault(record => record.Id == data.Id && record.UserId == userId)
            ?? await _luminaDbContext.MediaConsumptionRecords
                .FirstOrDefaultAsync(record => record.Id == data.Id && record.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (foundRecord is null)
            return DomainErrors.MediaConsumption.ConsumptionRecordNotFound;

        // A report that carries the values that are already stored is a no-op: writing it would bump the version for nothing, and a
        // concurrent stop or complete that read the same version could then fail for a report that changed no data.
        if (!HasEditableChanges(foundRecord, data))
            return Result.Updated;

        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        // The version is captured before the copy and incremented after it, so the update is only applied when the row still holds the version the report read.
        Guid storedUserId = foundRecord.UserId;
        Guid libraryId = foundRecord.LibraryId;
        Guid mediaItemId = foundRecord.MediaItemId;
        MediaKind mediaKind = foundRecord.MediaKind;
        DateTime startedOnUtc = foundRecord.StartedOnUtc;
        long version = foundRecord.Version;
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundRecord, data);
        foundRecord.UserId = storedUserId;
        foundRecord.LibraryId = libraryId;
        foundRecord.MediaItemId = mediaItemId;
        foundRecord.MediaKind = mediaKind;
        foundRecord.StartedOnUtc = startedOnUtc;
        foundRecord.Version = version + 1;
        return Result.Updated;
    }

    /// <summary>
    /// Deletes the consumption record identified by <paramref name="id"/> when it belongs to the user identified by <paramref name="userId"/>. A record that does not exist is not an error.
    /// </summary>
    /// <param name="id">The Id of the consumption record to delete.</param>
    /// <param name="userId">The Id of the user the consumption record must belong to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    public async Task<Result<Deleted>> DeleteForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        // A handler may already have loaded the same record in this unit of work, so the tracked instance is preferred over materializing a second copy.
        MediaConsumptionRecordEntity? record = _luminaDbContext.MediaConsumptionRecords.Local
            .FirstOrDefault(trackedRecord => trackedRecord.Id == id && trackedRecord.UserId == userId)
            ?? await _luminaDbContext.MediaConsumptionRecords
                .FirstOrDefaultAsync(foundRecord => foundRecord.Id == id && foundRecord.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (record is null)
            return Result.Deleted;

        _luminaDbContext.MediaConsumptionRecords.Remove(record);
        return Result.Deleted;
    }

    /// <summary>
    /// Deletes every consumption record of a media item of the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose consumption records are deleted.</param>
    /// <param name="mediaKind">The kind of the consumed media item.</param>
    /// <param name="mediaItemId">The Id of the consumed media item.</param>
    /// <param name="libraryId">The Id of the media library that contains the media item.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    public async Task<Result<Deleted>> DeleteAllByMediaItemIdForUserAsync(Guid userId, MediaKind mediaKind, Guid mediaItemId, Guid libraryId, CancellationToken cancellationToken)
    {
        List<MediaConsumptionRecordEntity> records = await _luminaDbContext.MediaConsumptionRecords
            .Where(record => record.UserId == userId &&
                             record.MediaKind == mediaKind &&
                             record.MediaItemId == mediaItemId &&
                             record.LibraryId == libraryId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (records.Count > 0)
            _luminaDbContext.MediaConsumptionRecords.RemoveRange(records);
        return Result.Deleted;
    }

    /// <summary>
    /// Deletes every consumption record of the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose consumption records are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error.</returns>
    public async Task<Result<Deleted>> DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<MediaConsumptionRecordEntity> records = await _luminaDbContext.MediaConsumptionRecords
            .Where(record => record.UserId == userId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (records.Count > 0)
            _luminaDbContext.MediaConsumptionRecords.RemoveRange(records);
        return Result.Deleted;
    }

    /// <summary>
    /// Determines whether <paramref name="incoming"/> carries values that differ from the stored <paramref name="stored"/> record.
    /// </summary>
    /// <param name="stored">The record currently stored in the persistence medium.</param>
    /// <param name="incoming">The record carrying the values to apply.</param>
    /// <returns><see langword="true"/> if an editable value changed, <see langword="false"/> otherwise.</returns>
    private static bool HasEditableChanges(MediaConsumptionRecordEntity stored, MediaConsumptionRecordEntity incoming)
    {
        return stored.Status != incoming.Status ||
               stored.LastActivityOnUtc != incoming.LastActivityOnUtc ||
               stored.EndedOnUtc != incoming.EndedOnUtc ||
               stored.ConsumedDurationMilliseconds != incoming.ConsumedDurationMilliseconds ||
               stored.LastPositionKind != incoming.LastPositionKind ||
               stored.LastPositionTimeOffsetMilliseconds != incoming.LastPositionTimeOffsetMilliseconds ||
               stored.LastPositionLocation != incoming.LastPositionLocation ||
               stored.LastProgressFraction != incoming.LastProgressFraction ||
               stored.ConsecutivePagesRead != incoming.ConsecutivePagesRead;
    }
}
