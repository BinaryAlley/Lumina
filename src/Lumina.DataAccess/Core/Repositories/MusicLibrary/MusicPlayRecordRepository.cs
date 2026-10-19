#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.DataAccess.Common.Persistence;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MusicLibrary;

/// <summary>
/// Repository for recorded music plays.
/// </summary>
internal sealed class MusicPlayRecordRepository : IMusicPlayRecordRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicPlayRecordRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public MusicPlayRecordRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

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
    public async Task<Result<MusicPlayRecordEntity?>> GetByIdForUserAsync(Guid id, Guid userId, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        // The user is part of the filter rather than a check performed after the read, so the storage medium never returns a record that
        // belongs to another user in the first place.
        IQueryable<MusicPlayRecordEntity> query = _luminaDbContext.MusicPlayRecords
            .Where(playRecord => playRecord.Id == id && playRecord.UserId == userId);
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a new play record.
    /// </summary>
    /// <param name="data">The play record to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(MusicPlayRecordEntity data, CancellationToken cancellationToken)
    {
        bool doesPlayRecordExist = await _luminaDbContext.MusicPlayRecords
            .AnyAsync(playRecord => playRecord.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (doesPlayRecordExist)
            return DomainErrors.MusicPlayHistory.MusicPlayRecordAlreadyExists;

        _luminaDbContext.MusicPlayRecords.Add(data);
        return Result.Created;
    }

    /// <summary>
    /// Updates the play record of the user identified by <paramref name="userId"/>, when the record belongs to that user and still holds the
    /// version it was read with.
    /// </summary>
    /// <param name="data">The play record whose editable values are applied to the stored record.</param>
    /// <param name="userId">The Id of the user the play record must belong to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public async Task<Result<Updated>> UpdateForUserAsync(MusicPlayRecordEntity data, Guid userId, CancellationToken cancellationToken)
    {
        // The user is part of both the tracked and the stored lookup, so a future caller can never update a record that belongs to another
        // user merely by knowing its Id.
        MusicPlayRecordEntity? foundPlayRecord = _luminaDbContext.MusicPlayRecords.Local
            .FirstOrDefault(playRecord => playRecord.Id == data.Id && playRecord.UserId == userId)
            ?? await _luminaDbContext.MusicPlayRecords
                .FirstOrDefaultAsync(playRecord => playRecord.Id == data.Id && playRecord.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (foundPlayRecord is null)
            return DomainErrors.MusicPlayHistory.MusicPlayRecordNotFound;

        // A report that carries the values that are already stored is a no-op: writing it would bump the version for nothing, and a
        // concurrent stop or complete that read the same version could then fail for a report that changed no data.
        bool hasEditableChanges = foundPlayRecord.Status != data.Status ||
                                  foundPlayRecord.LastPositionMilliseconds != data.LastPositionMilliseconds ||
                                  foundPlayRecord.ListenedDurationMilliseconds != data.ListenedDurationMilliseconds ||
                                  foundPlayRecord.EndedOnUtc != data.EndedOnUtc;
        if (!hasEditableChanges)
            return Result.Updated;

        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        // The version is captured before the copy and incremented after it, so the update is only applied when the row still holds the
        // version the report read; a later progress report can therefore never overwrite a completed or stopped play.
        Guid storedUserId = foundPlayRecord.UserId;
        Guid trackId = foundPlayRecord.TrackId;
        Guid libraryId = foundPlayRecord.LibraryId;
        Guid albumId = foundPlayRecord.AlbumId;
        Guid artistId = foundPlayRecord.ArtistId;
        long version = foundPlayRecord.Version;
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundPlayRecord, data);
        foundPlayRecord.UserId = storedUserId;
        foundPlayRecord.TrackId = trackId;
        foundPlayRecord.LibraryId = libraryId;
        foundPlayRecord.AlbumId = albumId;
        foundPlayRecord.ArtistId = artistId;
        foundPlayRecord.Version = version + 1;
        return Result.Updated;
    }
}
