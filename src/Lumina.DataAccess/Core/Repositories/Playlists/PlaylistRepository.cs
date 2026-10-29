#region ========================================================================= USING =====================================================================================
using Dapper;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.DataAccess.Common.Persistence;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Playlists;

/// <summary>
/// Repository for playlists.
/// </summary>
/// <remarks>
/// Entry mutations are set-based because a playlist can hold a whole library. Appends bypass the change tracker, so they maintain the audit
/// columns and the denormalized entry count themselves. Positions are sparse order keys enforced by the unique (PlaylistId, Position) index.
/// </remarks>
internal sealed class PlaylistRepository : IPlaylistRepository
{
    // Keeps the number of bound parameters of one multi-row INSERT well under the SQLite parameter limit.
    private const int INSERT_BATCH_SIZE = 500;
    // Leaves a wide numeric gap between adjacent entries, so a move or a removal normally writes one row instead of the whole range.
    private const long POSITION_STEP = 1_000_000_000L;

    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public PlaylistRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new playlist.
    /// </summary>
    /// <param name="data">The playlist to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(PlaylistEntity data, CancellationToken cancellationToken)
    {
        // The Id comes from the aggregate, so a row with this Id means the same playlist was already persisted, for example by a retried request.
        bool doesPlaylistExist = await _luminaDbContext.Playlists
            .AnyAsync(playlist => playlist.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (doesPlaylistExist)
            return DomainErrors.Playlists.PlaylistAlreadyExists;

        _luminaDbContext.Playlists.Add(data);
        return Result.Created;
    }

    /// <summary>
    /// Updates the playlist identified by <paramref name="data"/>, applying only the editable data that actually changed, while preserving its
    /// identity, its entries, and its audit columns.
    /// </summary>
    /// <param name="data">The playlist whose editable data is applied to the stored playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(PlaylistEntity data, CancellationToken cancellationToken)
    {
        // The playlist details are a scalar edit. Entry mutations are set-based repository operations, so this update never loads or reconciles the entry collection.
        // A handler may already have loaded the same playlist in this unit of work, so the tracked instance is preferred over materializing a second copy.
        PlaylistEntity? foundPlaylist = _luminaDbContext.Playlists.Local
            .FirstOrDefault(playlist => playlist.Id == data.Id && playlist.UserId == data.UserId);
        foundPlaylist ??= await _luminaDbContext.Playlists.FirstOrDefaultAsync(playlist => playlist.Id == data.Id && playlist.UserId == data.UserId, cancellationToken).ConfigureAwait(false);
        if (foundPlaylist is null)
            return DomainErrors.Playlists.PlaylistNotFound;

        // The stored identity is never overwritten by an edit. The audit columns stay under the ownership of the auditing interceptor,
        // and the version is incremented here so a concurrent details edit or entry operation is detected.
        Guid storedUserId = foundPlaylist.UserId;
        long storedVersion = foundPlaylist.Version;
        int storedEntryCount = foundPlaylist.EntryCount;
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundPlaylist, data);
        foundPlaylist.UserId = storedUserId;

        // The version and the entry count are not editable values of the aggregate. The count is maintained by the set-based entry
        // operations, so a details edit can never overwrite it with the count of the mapped entity.
        EntityEntry<PlaylistEntity> trackedEntry = _luminaDbContext.Entry(foundPlaylist);
        trackedEntry.Property(playlist => playlist.UserId).IsModified = false;
        trackedEntry.Property(playlist => playlist.Version).CurrentValue = storedVersion;
        trackedEntry.Property(playlist => playlist.EntryCount).CurrentValue = storedEntryCount;
        trackedEntry.Property(playlist => playlist.EntryCount).IsModified = false;

        // The version is only incremented when another editable value actually changed, so a save that leaves the details untouched does
        // not stamp an activity or a concurrency change that the user never made. The comparison against the tracked original values is
        // explicit, because no change detection has run between the copy and this check.
        bool hasEditableChanges = trackedEntry.Properties
            .Any(property => property.Metadata.Name != nameof(PlaylistEntity.Version)
                && property.Metadata.Name != nameof(PlaylistEntity.EntryCount)
                && !Equals(property.OriginalValue, property.CurrentValue));
        if (!hasEditableChanges)
        {
            trackedEntry.Property(playlist => playlist.Version).IsModified = false;
            return Result.Updated;
        }
        foundPlaylist.Version = storedVersion + 1;
        return Result.Updated;
    }

    /// <summary>
    /// Gets the playlist identified by <paramref name="id"/> when it belongs to the user identified by <paramref name="userId"/>, if it exists.
    /// </summary>
    /// <param name="id">The Id of the playlist to get.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the entries of the playlist should be loaded together with the playlist itself.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the playlist identified by <paramref name="id"/> and owned by <paramref name="userId"/>, <see langword="null"/> when no such playlist exists, or an error.
    /// </returns>
    public async Task<Result<PlaylistEntity?>> GetByIdForUserAsync(Guid id, Guid userId, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        // The user is part of the filter rather than a check performed after the read, so the storage medium never returns a playlist that belongs to another user in the first place.
        IQueryable<PlaylistEntity> query = _luminaDbContext.Playlists;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
            query = query.Include(playlist => playlist.Entries);
        return await query.FirstOrDefaultAsync(playlist => playlist.Id == id && playlist.UserId == userId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the playlists of the user identified by <paramref name="userId"/>, ordered by their most recent activity.
    /// </summary>
    /// <param name="userId">The Id of the user whose playlists are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the playlists of the user, or an error.</returns>
    public async Task<Result<IReadOnlyList<PlaylistSummaryDto>>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        IReadOnlyList<PlaylistSummaryDto> playlists = await _luminaDbContext.Playlists
            .AsNoTracking()
            .Where(playlist => playlist.UserId == userId)
            .OrderByDescending(playlist => playlist.UpdatedOnUtc ?? playlist.CreatedOnUtc)
            .ThenBy(playlist => playlist.Name)
            // The count is stored on the playlist row, so listing the playlists never counts every entry of every playlist.
            .Select(playlist => new PlaylistSummaryDto(
                playlist.Id,
                playlist.Name,
                playlist.Description,
                playlist.Type,
                playlist.EntryCount,
                playlist.CreatedOnUtc,
                playlist.UpdatedOnUtc))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.From(playlists);
    }

    /// <summary>
    /// Gets the playlist identified by <paramref name="id"/> together with a page of the display details of the media items its entries reference,
    /// when the playlist belongs to the user identified by <paramref name="userId"/>, or all the entries when the pagination data is <see langword="null"/>.
    /// </summary>
    /// <param name="id">The Id of the playlist to get.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching entries are returned.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the playlist details and the paginated entries, <see langword="null"/> when no such playlist exists, or an error.
    /// </returns>
    public async Task<Result<PlaylistDetailsDto?>> GetDetailsForUserAsync(Guid id, Guid userId, PaginationDataDto? paginationData = null, CancellationToken cancellationToken = default)
    {
        // The user is part of the filter rather than a check performed after the read, so the storage medium never returns a playlist that belongs to another user in the first place.
        PlaylistEntity? playlist = await _luminaDbContext.Playlists
            .AsNoTracking()
            .FirstOrDefaultAsync(foundPlaylist => foundPlaylist.Id == id && foundPlaylist.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (playlist is null)
            return Result<PlaylistDetailsDto?>.Success(null);

        IQueryable<PlaylistEntryEntity> entriesQuery = _luminaDbContext.PlaylistEntries
            .AsNoTracking()
            .Where(entry => entry.PlaylistId == id)
            .OrderBy(entry => entry.Position);

        // If no pagination was requested, return all the entries of the playlist.
        if (paginationData is null)
        {
            IReadOnlyList<PlaylistEntryDto> allEntries = await ToPlaylistEntriesAsync(entriesQuery, offset: 0, cancellationToken).ConfigureAwait(false);
            return new PlaylistDetailsDto(playlist, new PaginatedResultDto<PlaylistEntryDto>
            {
                Data = allEntries,
                CurrentPage = 1,
                PerPage = allEntries.Count,
                Count = allEntries.Count,
                NumberOfPages = 1
            });
        }

        int count = await entriesQuery.Select(entry => entry.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.
        int offset = (currentPage - 1) * paginationData.PerPage;

        // Only the requested page is materialized. A playlist can hold any number of entries, so the page never loads the whole list.
        IQueryable<PlaylistEntryEntity> pageQuery = entriesQuery
            .Skip(offset)
            .Take(paginationData.PerPage);
        IReadOnlyList<PlaylistEntryDto> paginatedResult = await ToPlaylistEntriesAsync(pageQuery, offset, cancellationToken).ConfigureAwait(false);

        return new PlaylistDetailsDto(playlist, new PaginatedResultDto<PlaylistEntryDto>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        });
    }

    /// <summary>
    /// Reads the display data of the tracks referenced by the entries of <paramref name="entriesQuery"/>.
    /// </summary>
    /// <param name="entriesQuery">The ordered query of the entries to read the display data for.</param>
    /// <param name="offset">The zero based position of the first entry of the page, used to compute the dense display position.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The entries together with the display data of the tracks they reference.</returns>
    private async Task<IReadOnlyList<PlaylistEntryDto>> ToPlaylistEntriesAsync(IQueryable<PlaylistEntryEntity> entriesQuery, int offset, CancellationToken cancellationToken)
    {
        List<PlaylistEntryEntity> entries = await entriesQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

        List<Guid> trackIds = [.. entries
            .Where(entry => entry.MediaItemType == PlaylistMediaItemType.MusicTrack)
            .Select(entry => entry.MediaItemId)
            .Distinct()];
        Dictionary<Guid, PlaylistEntryDisplayDto> displayDataByTrackId = await GetTrackDisplayDataAsync(trackIds, cancellationToken).ConfigureAwait(false);

        // An entry whose track was deleted is kept with no display data, so its owner can still see and remove it instead of it silently
        // disappearing from the playlist. The sparse order key is converted to a dense display position for the response.
        List<PlaylistEntryDto> items = [];
        for (int index = 0; index < entries.Count; index++)
        {
            PlaylistEntryEntity entry = entries[index];
            displayDataByTrackId.TryGetValue(entry.MediaItemId, out PlaylistEntryDisplayDto? displayData);
            items.Add(new PlaylistEntryDto(entry, displayData, offset + index));
        }
        return items;
    }

    /// <summary>
    /// Gets the entry that was just appended to a playlist of the user identified by <paramref name="userId"/>, together with the display details of the media item it references.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist the entry must belong to.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="entryId">The Id of the appended entry to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the entry details, <see langword="null"/> when the entry does not exist in that playlist of the user, or an error.
    /// </returns>
    public async Task<Result<PlaylistEntryDto?>> GetAppendedEntryDetailsForUserAsync(Guid playlistId, Guid userId, Guid entryId, CancellationToken cancellationToken)
    {
        // The user is part of the join, so the read never returns an entry of a playlist that belongs to another user.
        AppendedEntryState? state = await (from candidate in _luminaDbContext.PlaylistEntries.AsNoTracking()
                                           join playlist in _luminaDbContext.Playlists on candidate.PlaylistId equals playlist.Id
                                           where candidate.Id == entryId && candidate.PlaylistId == playlistId && playlist.UserId == userId
                                           select new AppendedEntryState
                                           {
                                               Entry = candidate,
                                               EntryCount = playlist.EntryCount
                                           })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (state is null)
            return Result<PlaylistEntryDto?>.Success(null);

        List<Guid> trackIds = state.Entry.MediaItemType == PlaylistMediaItemType.MusicTrack ? [state.Entry.MediaItemId] : [];
        Dictionary<Guid, PlaylistEntryDisplayDto> displayDataByTrackId = await GetTrackDisplayDataAsync(trackIds, cancellationToken).ConfigureAwait(false);
        displayDataByTrackId.TryGetValue(state.Entry.MediaItemId, out PlaylistEntryDisplayDto? displayData);
        // The appended entry is the last entry, so its dense display position is the stored entry count minus one. Counting every entry
        // before it would scan an index of the whole playlist for a value that the playlist row already holds.
        int displayPosition = Math.Max(0, state.EntryCount - 1);
        return new PlaylistEntryDto(state.Entry, displayData, displayPosition);
    }

    /// <summary>
    /// Appends a batch of media items of one kind to the end of the ordered entries of a playlist of the user identified by <paramref name="userId"/>,
    /// using batched bulk inserts, and updates the activity time, the entry count and the concurrency token of the playlist in the same transaction.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to append to.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="mediaItemIds">The Ids of the media items to append, in order. Duplicates are kept.</param>
    /// <param name="mediaItemType">The kind of media item referenced by every appended entry.</param>
    /// <param name="entryIds">The Ids to assign to the appended entries, in the same order as <paramref name="mediaItemIds"/>, or <see langword="null"/> to generate them.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the sparse order key of the first appended entry and the added count, or an error.
    /// </returns>
    public async Task<Result<PlaylistBulkAddResultDto>> AddEntriesAsync(Guid playlistId, Guid userId, IReadOnlyList<Guid> mediaItemIds, PlaylistMediaItemType mediaItemType, IReadOnlyList<Guid>? entryIds, DateTime updatedOnUtc, CancellationToken cancellationToken)
    {
        if (entryIds is not null && entryIds.Count != mediaItemIds.Count)
            return DomainErrors.Playlists.PlaylistEntryIdCannotBeEmpty;

        // The shared helper owns the playlist checks, the transaction, the activity update and the entry count. The callback only writes the rows.
        return await AppendEntriesAsync(
            playlistId,
            userId,
            updatedOnUtc,
            (connection, transaction, firstPosition, appendCancellationToken) => InsertEntryBatchesAsync(
                connection,
                transaction,
                ToStorageId(playlistId),
                ToStorageId(userId),
                firstPosition,
                mediaItemIds,
                entryIds,
                mediaItemType,
                updatedOnUtc,
                appendCancellationToken),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Appends every media item of a library, an artist or an album to the end of the ordered entries of a playlist of the user identified by <paramref name="userId"/>, in catalogue order, without materializing the scope.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to append to.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="libraryId">The Id of the library whose media items are appended.</param>
    /// <param name="artistId">The Id of the artist the scope is limited to, or <see langword="null"/> for every artist of the library.</param>
    /// <param name="albumId">The Id of the album the scope is limited to, or <see langword="null"/> for every album of the scope.</param>
    /// <param name="mediaItemType">The kind of media item referenced by every appended entry.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the sparse order key of the first appended entry and the added count, or an error.
    /// </returns>
    public async Task<Result<PlaylistBulkAddResultDto>> AddScopeEntriesAsync(Guid playlistId, Guid userId, Guid libraryId, Guid? artistId, Guid? albumId, PlaylistMediaItemType mediaItemType, DateTime updatedOnUtc, CancellationToken cancellationToken)
    {
        // The callback inserts the scope with SQL instead of receiving a list, which is what keeps a whole-library append from materializing every track Id before the insert starts.
        return await AppendEntriesAsync(
            playlistId,
            userId,
            updatedOnUtc,
            (connection, transaction, firstPosition, appendCancellationToken) => InsertScopeEntriesAsync(
                connection,
                transaction,
                ToStorageId(playlistId),
                ToStorageId(userId),
                firstPosition,
                libraryId,
                artistId,
                albumId,
                mediaItemType,
                updatedOnUtc,
                appendCancellationToken),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Appends entries to a playlist with a set-based operation, and updates the activity time, the entry count and the concurrency token of the playlist in the same transaction.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to append to.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="appendEntries">
    /// The operation that inserts the entries and returns the number of rows it inserted, using the connection, the transaction and the
    /// sparse order key of the first appended entry supplied by this method.
    /// </param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the sparse order key of the first appended entry and the added count, or an error.
    /// </returns>
    private async Task<Result<PlaylistBulkAddResultDto>> AppendEntriesAsync(
        Guid playlistId,
        Guid userId,
        DateTime updatedOnUtc,
        Func<DbConnection, DbTransaction, long, CancellationToken, Task<int>> appendEntries,
        CancellationToken cancellationToken)
    {
        // The Ids are written in the same upper case TEXT representation the Entity Framework provider uses, so the raw inserts join the
        // existing rows, their indexes and their foreign keys consistently. The registered Dapper Guid type handler uses the same form.
        string playlistIdText = ToStorageId(playlistId);
        string userIdText = ToStorageId(userId);

        // The raw inserts run on the connection and transaction of the request's unit of work, so they participate in the same
        // atomic operation as the rest of the command instead of opening a second connection that would fight for the SQLite write lock.
        DbConnection connection = _luminaDbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        DbTransaction? existingTransaction = _luminaDbContext.Database.CurrentTransaction?.GetDbTransaction();
        DbTransaction transaction = existingTransaction ?? await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        // The request middleware owns the transaction it opened and decides its fate. The repository only commits or rolls back a
        // transaction it opened itself, which happens when it runs without an ambient request transaction.
        bool ownsTransaction = existingTransaction is null;

        try
        {
            // The validation failures below happen before any row is written. There is therefore nothing for an ambient request transaction to roll back, so only a transaction opened here is closed.
            // The append only needs to know that the playlist still exists for this user.
            const string PLAYLIST_EXISTS_SQL = "SELECT 1 FROM Playlists WHERE Id = @playlistId AND UserId = @userId LIMIT 1;";
            CommandDefinition playlistExistsCommand = new(PLAYLIST_EXISTS_SQL, new { playlistId = playlistIdText, userId = userIdText }, transaction, cancellationToken: cancellationToken);
            long? playlistExists = await connection.ExecuteScalarAsync<long?>(playlistExistsCommand).ConfigureAwait(false);
            if (playlistExists is null)
            {
                await RollbackOwnedTransactionAsync(transaction, ownsTransaction).ConfigureAwait(false);
                return DomainErrors.Playlists.PlaylistNotFound;
            }

            // The append starts one sparse step after the highest stored key. MAX seeks the (PlaylistId, Position) index; COUNT(*) would scan the playlist for every single add.
            const string FIRST_POSITION_SQL = "SELECT COALESCE(MAX(Position) + @step, 0) FROM PlaylistEntries WHERE PlaylistId = @playlistId;";
            CommandDefinition firstPositionCommand = new(FIRST_POSITION_SQL, new { playlistId = playlistIdText, step = POSITION_STEP }, transaction, cancellationToken: cancellationToken);
            long firstPosition = await connection.ExecuteScalarAsync<long>(firstPositionCommand).ConfigureAwait(false);

            int addedCount = await appendEntries(connection, transaction, firstPosition, cancellationToken).ConfigureAwait(false);
            if (addedCount == 0)
            {
                // An empty scope is a successful no-op. The playlist activity, version and count must not change just because nothing was added.
                if (ownsTransaction)
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new PlaylistBulkAddResultDto(firstPosition, 0);
            }

            // The count is updated in the same statement, so a window read never counts the entries of a whole playlist.
            const string UPDATE_ACTIVITY_SQL = """
                UPDATE Playlists
                SET UpdatedOnUtc = @updatedOnUtc, UpdatedBy = @userId, Version = Version + 1, EntryCount = EntryCount + @addedCount
                WHERE Id = @playlistId AND UserId = @userId;
                """;
            CommandDefinition updateActivityCommand = new(UPDATE_ACTIVITY_SQL, new { updatedOnUtc, playlistId = playlistIdText, userId = userIdText, addedCount }, transaction, cancellationToken: cancellationToken);
            int updatedPlaylistCount = await connection.ExecuteAsync(updateActivityCommand).ConfigureAwait(false);
            if (updatedPlaylistCount == 0)
            {
                // The entries were already written. A transaction opened here is rolled back before the error is returned; the request
                // transaction is left to the middleware, which rolls it back because this command answers with a failure.
                await RollbackOwnedTransactionAsync(transaction, ownsTransaction).ConfigureAwait(false);
                return DomainErrors.Playlists.PlaylistNotFound;
            }

            // The request middleware owns the transaction and commits it after the response, so only a transaction opened here is committed here.
            if (ownsTransaction)
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new PlaylistBulkAddResultDto(firstPosition, addedCount);
        }
        catch (SqliteException exception) when (IsUniqueConstraintViolation(exception))
        {
            await RollbackOwnedTransactionAsync(transaction, ownsTransaction).ConfigureAwait(false);
            return ApplicationErrors.Persistence.UniqueConstraintViolation;
        }
        catch (SqliteException exception) when (IsConcurrencyConflict(exception))
        {
            await RollbackOwnedTransactionAsync(transaction, ownsTransaction).ConfigureAwait(false);
            return ApplicationErrors.Persistence.ConcurrencyConflict;
        }
        catch (OperationCanceledException)
        {
            // A client abort is reported as a non-success status by the exception handling middleware, so the request transaction is rolled
            // back there. A transaction opened here still has to be closed before the exception propagates.
            await RollbackOwnedTransactionAsync(transaction, ownsTransaction).ConfigureAwait(false);
            throw;
        }
        catch
        {
            // Any other failure also leaves a transaction opened here unusable, so it is closed before the failure propagates. The request
            // transaction is left to the middleware, which rolls it back because the error answers with a non-success status.
            await RollbackOwnedTransactionAsync(transaction, ownsTransaction).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Inserts explicit media item Ids in bounded batches, so a long queue does not build one enormous SQL statement.
    /// </summary>
    /// <param name="connection">The open connection that carries the append transaction.</param>
    /// <param name="transaction">The transaction the inserts run in.</param>
    /// <param name="playlistIdText">The storage representation of the playlist Id.</param>
    /// <param name="userIdText">The storage representation of the user Id.</param>
    /// <param name="firstPosition">The sparse order key of the first appended entry.</param>
    /// <param name="mediaItemIds">The Ids of the media items to append, in order. Duplicates are kept.</param>
    /// <param name="entryIds">The Ids to assign to the appended entries, or <see langword="null"/> to generate them.</param>
    /// <param name="mediaItemType">The kind of media item referenced by every appended entry.</param>
    /// <param name="updatedOnUtc">The activity time to stamp on the appended entries.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The number of inserted entries.</returns>
    private static async Task<int> InsertEntryBatchesAsync(
        DbConnection connection,
        DbTransaction transaction,
        string playlistIdText,
        string userIdText,
        long firstPosition,
        IReadOnlyList<Guid> mediaItemIds,
        IReadOnlyList<Guid>? entryIds,
        PlaylistMediaItemType mediaItemType,
        DateTime updatedOnUtc,
        CancellationToken cancellationToken)
    {
        for (int offset = 0; offset < mediaItemIds.Count; offset += INSERT_BATCH_SIZE)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int count = Math.Min(INSERT_BATCH_SIZE, mediaItemIds.Count - offset);
            StringBuilder insertSql = new("INSERT INTO PlaylistEntries (Id, PlaylistId, MediaItemId, MediaItemType, Position, CreatedOnUtc, CreatedBy) VALUES ");
            DynamicParameters parameters = new();
            parameters.Add("mediaItemType", mediaItemType.ToString());
            parameters.Add("createdOnUtc", updatedOnUtc);
            parameters.Add("createdBy", userIdText);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                    insertSql.Append(", ");
                insertSql.Append($"(@Id{index}, @PlaylistId{index}, @MediaItemId{index}, @mediaItemType, @Position{index}, @createdOnUtc, @createdBy)");
                Guid entryId = entryIds is null ? Guid.NewGuid() : entryIds[offset + index];
                parameters.Add($"Id{index}", ToStorageId(entryId));
                parameters.Add($"PlaylistId{index}", playlistIdText);
                parameters.Add($"MediaItemId{index}", ToStorageId(mediaItemIds[offset + index]));
                // Every appended entry is one sparse step after the previous one, so later moves can change a single key without rewriting the entries that follow it.
                parameters.Add($"Position{index}", firstPosition + ((offset + index) * POSITION_STEP));
            }
            CommandDefinition insertCommand = new(insertSql.ToString(), parameters, transaction, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(insertCommand).ConfigureAwait(false);
        }
        // The caller uses this count to update the stored entry count, so it returns the requested rows rather than the number of batches.
        return mediaItemIds.Count;
    }

    /// <summary>
    /// Inserts every track of a library, an artist or an album with one INSERT ... SELECT statement, in catalogue order, so the scope is
    /// never materialized and sorted in memory.
    /// </summary>
    /// <param name="connection">The open connection that carries the append transaction.</param>
    /// <param name="transaction">The transaction the insert runs in.</param>
    /// <param name="playlistIdText">The storage representation of the playlist Id.</param>
    /// <param name="userIdText">The storage representation of the user Id.</param>
    /// <param name="firstPosition">The sparse order key of the first appended entry.</param>
    /// <param name="libraryId">The Id of the library whose tracks are appended.</param>
    /// <param name="artistId">The Id of the artist the scope is limited to, or <see langword="null"/> for every artist of the library.</param>
    /// <param name="albumId">The Id of the album the scope is limited to, or <see langword="null"/> for every album of the scope.</param>
    /// <param name="mediaItemType">The kind of media item referenced by every appended entry.</param>
    /// <param name="updatedOnUtc">The activity time to stamp on the appended entries.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The number of inserted entries.</returns>
    private static async Task<int> InsertScopeEntriesAsync(
        DbConnection connection,
        DbTransaction transaction,
        string playlistIdText,
        string userIdText,
        long firstPosition,
        Guid libraryId,
        Guid? artistId,
        Guid? albumId,
        PlaylistMediaItemType mediaItemType,
        DateTime updatedOnUtc,
        CancellationToken cancellationToken)
    {
        // The entry Ids are generated by SQLite in the same upper case, hyphenated TEXT form the Entity Framework provider stores. Generating
        // them inside the statement is what lets the scope stay a single insert instead of being materialized in C# just to assign Ids.
        // The positions are computed by the same ordering the artist and album pages use, so the playlist receives catalogue order directly.
        // ROW_NUMBER() produces the dense rank, and multiplying it by POSITION_STEP keeps the stored keys sparse.
        StringBuilder insertSql = new("""
            INSERT INTO PlaylistEntries (Id, PlaylistId, MediaItemId, MediaItemType, Position, CreatedOnUtc, CreatedBy)
            SELECT
                HEX(RANDOMBLOB(4)) || '-' || HEX(RANDOMBLOB(2)) || '-' || HEX(RANDOMBLOB(2)) || '-' || HEX(RANDOMBLOB(2)) || '-' || HEX(RANDOMBLOB(6)),
                @playlistId,
                track.Id,
                @mediaItemType,
                @firstPosition + (ROW_NUMBER() OVER (ORDER BY artist.Name, album.Title, album.OriginalReleaseYear, COALESCE(track.DiscNumber, 1), track.TrackNumber, track.Id) - 1) * @step,
                @createdOnUtc,
                @createdBy
            FROM Tracks AS track
            INNER JOIN Albums AS album ON album.Id = track.AlbumId
            INNER JOIN Artists AS artist ON artist.Id = album.ArtistId
            WHERE track.LibraryId = @libraryId
            """);
        if (artistId is not null)
            insertSql.Append(" AND album.ArtistId = @artistId");
        if (albumId is not null)
            insertSql.Append(" AND track.AlbumId = @albumId");
        insertSql.Append(';');

        DynamicParameters parameters = new();
        parameters.Add("playlistId", playlistIdText);
        parameters.Add("libraryId", ToStorageId(libraryId));
        parameters.Add("mediaItemType", mediaItemType.ToString());
        parameters.Add("firstPosition", firstPosition);
        parameters.Add("step", POSITION_STEP);
        parameters.Add("createdOnUtc", updatedOnUtc);
        parameters.Add("createdBy", userIdText);
        if (artistId is not null)
            parameters.Add("artistId", ToStorageId(artistId.Value));
        if (albumId is not null)
            parameters.Add("albumId", ToStorageId(albumId.Value));

        CommandDefinition insertCommand = new(insertSql.ToString(), parameters, transaction, cancellationToken: cancellationToken);
        return await connection.ExecuteAsync(insertCommand).ConfigureAwait(false);
    }

    /// <summary>
    /// Moves one entry of a playlist of the user identified by <paramref name="userId"/> to a new dense position by changing only its
    /// sparse order key, and updates the activity time and the concurrency token of the playlist in the same transaction.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist whose entry is moved.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="entryId">The Id of the entry to move.</param>
    /// <param name="targetPosition">The new zero based position of the entry.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful move, or an error.</returns>
    public async Task<Result<Updated>> MoveEntryAsync(Guid playlistId, Guid userId, Guid entryId, int targetPosition, DateTime updatedOnUtc, CancellationToken cancellationToken)
    {
        try
        {
            // The version, the entry count and the existence of the entry are read in one statement. The count comes from the playlist
            // row, so a drag on a very large playlist never scans the entry index to find out how many entries it holds. Keeping the
            // existence check in the same query also preserves the not-found result for a one-entry playlist, where no entry update runs.
            PlaylistMoveState? playlistState = await _luminaDbContext.Playlists
                .AsNoTracking()
                .Where(playlist => playlist.Id == playlistId && playlist.UserId == userId)
                .Select(playlist => new PlaylistMoveState
                {
                    Version = playlist.Version,
                    EntryCount = playlist.EntryCount,
                    DoesEntryExist = _luminaDbContext.PlaylistEntries.Any(entry => entry.Id == entryId && entry.PlaylistId == playlistId)
                })
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (playlistState is null)
                return DomainErrors.Playlists.PlaylistNotFound;
            if (!playlistState.DoesEntryExist)
                return DomainErrors.Playlists.PlaylistEntryNotFound;

            long playlistVersion = playlistState.Version;
            int entryCount = playlistState.EntryCount;
            if (targetPosition < 0 || targetPosition >= entryCount)
                return DomainErrors.Playlists.PlaylistMoveTargetPositionIsInvalid;

            // A one-entry playlist has no order to change, so only the activity time is updated below.
            if (entryCount > 1)
            {
                long newPosition;
                if (targetPosition == 0)
                {
                    // Moving to the front needs a sparse key before the current first entry.
                    long? nextPosition = await _luminaDbContext.PlaylistEntries
                        .AsNoTracking()
                        .Where(entry => entry.PlaylistId == playlistId && entry.Id != entryId)
                        .OrderBy(entry => entry.Position)
                        .Select(entry => (long?)entry.Position)
                        .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                    if (nextPosition is null)
                        return ApplicationErrors.Persistence.ConcurrencyConflict;
                    newPosition = nextPosition.Value - POSITION_STEP;
                }
                else if (targetPosition == entryCount - 1)
                {
                    // Moving to the end needs a sparse key after the current last entry.
                    long? previousPosition = await _luminaDbContext.PlaylistEntries
                        .AsNoTracking()
                        .Where(entry => entry.PlaylistId == playlistId && entry.Id != entryId)
                        .OrderByDescending(entry => entry.Position)
                        .Select(entry => (long?)entry.Position)
                        .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                    if (previousPosition is null)
                        return ApplicationErrors.Persistence.ConcurrencyConflict;
                    newPosition = previousPosition.Value + POSITION_STEP;
                }
                else
                {
                    // A middle move only needs the two rows that will surround the moved entry. Its new key is the midpoint between them.
                    List<long> neighbors = await GetMoveNeighborsAsync(playlistId, entryId, targetPosition, cancellationToken).ConfigureAwait(false);
                    if (neighbors.Count < 2)
                        return ApplicationErrors.Persistence.ConcurrencyConflict;
                    if (neighbors[1] - neighbors[0] <= 1)
                    {
                        // The numeric gap between the two neighbors was exhausted by earlier midpoint moves. This is the rare repair path;
                        // it rewrites the order keys once, then the move continues with a fresh gap.
                        await RebalancePositionsAsync(playlistId, cancellationToken).ConfigureAwait(false);
                        neighbors = await GetMoveNeighborsAsync(playlistId, entryId, targetPosition, cancellationToken).ConfigureAwait(false);
                        if (neighbors.Count < 2)
                            return ApplicationErrors.Persistence.ConcurrencyConflict;
                    }
                    long gap = neighbors[1] - neighbors[0];
                    if (gap <= 1)
                        return ApplicationErrors.Persistence.ConcurrencyConflict;
                    newPosition = neighbors[0] + (gap / 2);
                }

                int updatedEntryCount = await _luminaDbContext.PlaylistEntries
                    .Where(entry => entry.Id == entryId && entry.PlaylistId == playlistId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(entry => entry.Position, newPosition)
                        .SetProperty(entry => entry.UpdatedOnUtc, updatedOnUtc)
                        .SetProperty(entry => entry.UpdatedBy, (Guid?)userId), cancellationToken).ConfigureAwait(false);
                if (updatedEntryCount == 0)
                    return DomainErrors.Playlists.PlaylistEntryNotFound;
            }

            int updatedPlaylistCount = await UpdatePlaylistActivityAsync(playlistId, userId, playlistVersion, updatedOnUtc, entryCountDelta: 0, cancellationToken).ConfigureAwait(false);
            if (updatedPlaylistCount == 0)
                return ApplicationErrors.Persistence.ConcurrencyConflict;
            return Result.Updated;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqliteException && IsUniqueConstraintViolation(sqliteException))
        {
            return ApplicationErrors.Persistence.UniqueConstraintViolation;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqliteException && IsConcurrencyConflict(sqliteException))
        {
            return ApplicationErrors.Persistence.ConcurrencyConflict;
        }
        catch (SqliteException exception) when (IsUniqueConstraintViolation(exception))
        {
            return ApplicationErrors.Persistence.UniqueConstraintViolation;
        }
        catch (SqliteException exception) when (IsConcurrencyConflict(exception))
        {
            return ApplicationErrors.Persistence.ConcurrencyConflict;
        }
    }

    /// <summary>
    /// Removes one entry of a playlist of the user identified by <paramref name="userId"/> by deleting only that row, and updates the
    /// activity time, the entry count and the concurrency token of the playlist in the same transaction.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist whose entry is removed.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="entryId">The Id of the entry to remove.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either zero repositioned entries, or an error.</returns>
    public async Task<Result<int>> RemoveEntryAsync(Guid playlistId, Guid userId, Guid entryId, DateTime updatedOnUtc, CancellationToken cancellationToken)
    {
        try
        {
            // The version is captured before the delete, so the following count update only applies if no other writer changed the playlist meanwhile.
            long? playlistVersion = await _luminaDbContext.Playlists
                .AsNoTracking()
                .Where(playlist => playlist.Id == playlistId && playlist.UserId == userId)
                .Select(playlist => (long?)playlist.Version)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (playlistVersion is null)
                return DomainErrors.Playlists.PlaylistNotFound;

            // The delete reports how many rows it removed, so a missing or already removed entry needs no separate existence query.
            int deletedEntryCount = await _luminaDbContext.PlaylistEntries
                .Where(entry => entry.Id == entryId && entry.PlaylistId == playlistId)
                .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            if (deletedEntryCount == 0)
                return DomainErrors.Playlists.PlaylistEntryNotFound;

            // The deleted row decrements the stored count by one. The version predicate ties that decrement to the version read above, so a
            // concurrent writer is reported instead of leaving the count out of step with the entries.
            int updatedPlaylistCount = await UpdatePlaylistActivityAsync(playlistId, userId, playlistVersion.Value, updatedOnUtc, entryCountDelta: -1, cancellationToken).ConfigureAwait(false);
            if (updatedPlaylistCount == 0)
                return ApplicationErrors.Persistence.ConcurrencyConflict;
            return 0;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqliteException && IsUniqueConstraintViolation(sqliteException))
        {
            return ApplicationErrors.Persistence.UniqueConstraintViolation;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqliteException && IsConcurrencyConflict(sqliteException))
        {
            return ApplicationErrors.Persistence.ConcurrencyConflict;
        }
        catch (SqliteException exception) when (IsUniqueConstraintViolation(exception))
        {
            return ApplicationErrors.Persistence.UniqueConstraintViolation;
        }
        catch (SqliteException exception) when (IsConcurrencyConflict(exception))
        {
            return ApplicationErrors.Persistence.ConcurrencyConflict;
        }
    }

    /// <summary>
    /// Gets the two entries that surround the dense target position, excluding the entry being moved.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist.</param>
    /// <param name="entryId">The Id of the entry being moved.</param>
    /// <param name="targetPosition">The dense target position of the moved entry.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The sparse order keys of the previous and next entries at the target position.</returns>
    private async Task<List<long>> GetMoveNeighborsAsync(Guid playlistId, Guid entryId, int targetPosition, CancellationToken cancellationToken)
    {
        // The moved entry is excluded from the ordering, so the dense target position maps to the two rows before and after it.
        return await _luminaDbContext.PlaylistEntries
            .AsNoTracking()
            .Where(entry => entry.PlaylistId == playlistId && entry.Id != entryId)
            .OrderBy(entry => entry.Position)
            .Skip(targetPosition - 1)
            .Take(2)
            .Select(entry => entry.Position)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Rewrites the sparse order keys of a playlist to evenly spaced values.
    /// </summary>
    /// <remarks>
    /// This is a rare repair path, used only when repeated midpoint moves exhaust the numeric gap between two adjacent entries. It runs on
    /// the request connection and transaction, parks every row at a unique negative key, and then applies the new keys from a temporary table
    /// so the unique index is never violated during the rewrite.
    /// </remarks>
    /// <param name="playlistId">The Id of the playlist whose order keys are rewritten.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RebalancePositionsAsync(Guid playlistId, CancellationToken cancellationToken)
    {
        string playlistIdText = ToStorageId(playlistId);
        DbConnection connection = _luminaDbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        DbTransaction? existingTransaction = _luminaDbContext.Database.CurrentTransaction?.GetDbTransaction();
        DbTransaction transaction = existingTransaction ?? await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        bool ownsTransaction = existingTransaction is null;
        try
        {
            const string MIN_POSITION_SQL = "SELECT MIN(Position) FROM PlaylistEntries WHERE PlaylistId = @playlistId;";
            CommandDefinition minPositionCommand = new(MIN_POSITION_SQL, new { playlistId = playlistIdText }, transaction, cancellationToken: cancellationToken);
            long? minPosition = await connection.ExecuteScalarAsync<long?>(minPositionCommand).ConfigureAwait(false);
            if (minPosition is null)
            {
                if (ownsTransaction)
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            const string DROP_TEMP_TABLE_SQL = "DROP TABLE IF EXISTS PlaylistPositionRebalance;";
            const string CREATE_TEMP_TABLE_SQL = "CREATE TEMP TABLE PlaylistPositionRebalance (Id TEXT NOT NULL PRIMARY KEY, Position INTEGER NOT NULL, ParkPosition INTEGER NOT NULL);";
            // The final keys and a temporary unique negative key are computed once, in playlist order.
            const string FILL_TEMP_TABLE_SQL = """
                INSERT INTO PlaylistPositionRebalance (Id, Position, ParkPosition)
                SELECT Id,
                       (ROW_NUMBER() OVER (ORDER BY Position) - 1) * @step,
                       @minPosition - ROW_NUMBER() OVER (ORDER BY Position)
                FROM PlaylistEntries
                WHERE PlaylistId = @playlistId;
                """;
            // Every row is first moved to its unique negative key, so applying the final keys cannot collide with the unique index.
            const string PARK_SQL = """
                UPDATE PlaylistEntries
                SET Position = (
                    SELECT ParkPosition
                    FROM PlaylistPositionRebalance
                    WHERE PlaylistPositionRebalance.Id = PlaylistEntries.Id
                )
                WHERE PlaylistId = @playlistId;
                """;
            // The negative keys are then replaced by the final evenly spaced keys.
            const string APPLY_SQL = """
                UPDATE PlaylistEntries
                SET Position = (
                    SELECT Position
                    FROM PlaylistPositionRebalance
                    WHERE PlaylistPositionRebalance.Id = PlaylistEntries.Id
                )
                WHERE PlaylistId = @playlistId;
                """;
            object parameters = new { playlistId = playlistIdText, step = POSITION_STEP, minPosition = minPosition.Value };
            List<CommandDefinition> commands =
            [
                new(DROP_TEMP_TABLE_SQL, transaction: transaction, cancellationToken: cancellationToken),
                new(CREATE_TEMP_TABLE_SQL, transaction: transaction, cancellationToken: cancellationToken),
                new(FILL_TEMP_TABLE_SQL, parameters, transaction, cancellationToken: cancellationToken),
                new(PARK_SQL, parameters, transaction, cancellationToken: cancellationToken),
                new(APPLY_SQL, parameters, transaction, cancellationToken: cancellationToken),
                new(DROP_TEMP_TABLE_SQL, transaction: transaction, cancellationToken: cancellationToken)
            ];
            foreach (CommandDefinition command in commands)
                await connection.ExecuteAsync(command).ConfigureAwait(false);
            if (ownsTransaction)
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            if (ownsTransaction)
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Deletes the playlist identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the playlist to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        PlaylistEntity? playlist = await _luminaDbContext.Playlists
            .FirstOrDefaultAsync(foundPlaylist => foundPlaylist.Id == id, cancellationToken).ConfigureAwait(false);
        if (playlist is null)
            return DomainErrors.Playlists.PlaylistNotFound;

        _luminaDbContext.Playlists.Remove(playlist);
        return Result.Deleted;
    }

    /// <summary>
    /// Updates the activity time, the entry count and the concurrency token of the playlist identified by <paramref name="playlistId"/>, when its version still matches <paramref name="expectedVersion"/>.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to update.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="expectedVersion">The concurrency token the playlist must still carry.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="entryCountDelta">The number of entries the operation added to or removed from the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The number of playlist rows that were updated.</returns>
    private async Task<int> UpdatePlaylistActivityAsync(Guid playlistId, Guid userId, long expectedVersion, DateTime updatedOnUtc, int entryCountDelta, CancellationToken cancellationToken)
    {
        // ExecuteUpdate sends one UPDATE and never loads the entries or the playlist. The version predicate is the optimistic concurrency
        // check, and the returned row count tells the caller whether another writer won the race.
        return await _luminaDbContext.Playlists
            .Where(playlist => playlist.Id == playlistId && playlist.UserId == userId && playlist.Version == expectedVersion)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(playlist => playlist.UpdatedOnUtc, updatedOnUtc)
                .SetProperty(playlist => playlist.UpdatedBy, (Guid?)userId)
                .SetProperty(playlist => playlist.EntryCount, playlist => playlist.EntryCount + entryCountDelta)
                .SetProperty(playlist => playlist.Version, playlist => playlist.Version + 1), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Rolls back a transaction that this repository opened, leaving the request's ambient transaction untouched when it did not open one.
    /// </summary>
    /// <remarks>
    /// The request middleware owns the transaction it opened, and it commits or rolls it back once the response status is known. The repository
    /// only closes a transaction it opened itself, so it never takes a decision about work that belongs to the request.
    /// </remarks>
    /// <param name="transaction">The transaction to roll back.</param>
    /// <param name="ownsTransaction">Whether this repository opened the transaction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task RollbackOwnedTransactionAsync(DbTransaction transaction, bool ownsTransaction)
    {
        if (!ownsTransaction)
            return;
        // Cleanup still has to run when the request token has already been cancelled, which is exactly the failure path that reaches here.
        await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads the display details of the tracks identified by <paramref name="trackIds"/> with their album, artist and cover, in a single
    /// query per bounded chunk.
    /// </summary>
    /// <param name="trackIds">The Ids of the tracks whose display details are read.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The display details keyed by track Id.</returns>
    private async Task<Dictionary<Guid, PlaylistEntryDisplayDto>> GetTrackDisplayDataAsync(IReadOnlyCollection<Guid> trackIds, CancellationToken cancellationToken)
    {
        if (trackIds.Count == 0)
            return [];

        // The album and the artist are joined by Id, so a playlist never issues a query per entry. The cover is the one stored for the album
        // of the track. The lookup is split into bounded chunks, so even a large requested window never exceeds the storage parameter limit.
        const int LOOKUP_CHUNK_SIZE = 500;
        List<PlaylistEntryDisplayDto> trackDetails = [];
        foreach (Guid[] chunk in trackIds.Chunk(LOOKUP_CHUNK_SIZE))
        {
            List<PlaylistEntryDisplayDto> chunkDetails = await (from track in _luminaDbContext.Tracks.AsNoTracking()
                                                                    join album in _luminaDbContext.Albums on track.AlbumId equals album.Id
                                                                    join artist in _luminaDbContext.Artists on album.ArtistId equals artist.Id
                                                                    where chunk.Contains(track.Id)
                                                                    select new PlaylistEntryDisplayDto(
                                                                        track.Id,
                                                                        track.Title,
                                                                        track.DurationInSeconds,
                                                                        track.LibraryId,
                                                                        track.AlbumId,
                                                                        album.Title,
                                                                        artist.Id,
                                                                        artist.Name,
                                                                        _luminaDbContext.MusicArtwork
                                                                            .Where(artwork => artwork.OwnerType == MusicArtworkOwnerType.Album
                                                                                && artwork.OwnerId == album.Id
                                                                                && artwork.ArtworkType == ArtworkType.Cover)
                                                                            .Select(artwork => artwork.FileName)
                                                                            .FirstOrDefault()))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            trackDetails.AddRange(chunkDetails);
        }
        return trackDetails.ToDictionary(track => track.TrackId);
    }

    /// <summary>
    /// Determines whether <paramref name="exception"/> reports a unique constraint violation.
    /// </summary>
    /// <param name="exception">The storage exception to inspect.</param>
    /// <returns><see langword="true"/> for a unique constraint violation, <see langword="false"/> otherwise.</returns>
    private static bool IsUniqueConstraintViolation(SqliteException exception)
    {
        return exception.SqliteExtendedErrorCode == 2067; // SQLITE_CONSTRAINT_UNIQUE
    }

    /// <summary>
    /// Determines whether <paramref name="exception"/> reports a busy or locked SQLite database.
    /// </summary>
    /// <param name="exception">The storage exception to inspect.</param>
    /// <returns><see langword="true"/> for a busy or locked database, <see langword="false"/> otherwise.</returns>
    private static bool IsConcurrencyConflict(SqliteException exception)
    {
        return exception.SqliteErrorCode is 5 or 6; // SQLITE_BUSY, SQLITE_LOCKED
    }

    /// <summary>
    /// Converts an Id to the TEXT representation used by the Entity Framework SQLite provider for <see cref="Guid"/> columns.
    /// </summary>
    /// <remarks>
    /// Entity Framework and the Dapper parameter binding both write <see cref="Guid"/> values as upper case TEXT.
    /// </remarks>
    /// <param name="value">The Id to convert.</param>
    /// <returns>The upper case TEXT representation of the Id.</returns>
    private static string ToStorageId(Guid value)
    {
        return value.ToString().ToUpperInvariant();
    }

    /// <summary>
    /// The concurrency token and entry count of a playlist, together with whether a specific entry exists in it.
    /// </summary>
    private sealed class PlaylistMoveState
    {
        /// <summary>
        /// Gets or sets the concurrency token of the playlist.
        /// </summary>
        public long Version { get; init; }

        /// <summary>
        /// Gets or sets the number of entries the playlist holds.
        /// </summary>
        public int EntryCount { get; init; }

        /// <summary>
        /// Gets or sets whether the moved entry exists in the playlist.
        /// </summary>
        public bool DoesEntryExist { get; init; }
    }

    /// <summary>
    /// The appended entry and the stored entry count of its playlist.
    /// </summary>
    private sealed class AppendedEntryState
    {
        /// <summary>
        /// Gets or sets the appended entry.
        /// </summary>
        public PlaylistEntryEntity Entry { get; init; } = null!;

        /// <summary>
        /// Gets or sets the number of entries the playlist holds after the append.
        /// </summary>
        public int EntryCount { get; init; }
    }
}
