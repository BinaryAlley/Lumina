#region ========================================================================= USING =====================================================================================
using Dapper;
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.Infrastructure.Models.MediaLibraryScanJobPayloads;
using Lumina.DataAccess.Core.UoW;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Libraries;

/// <summary>
/// Repository for media library scan staging results.
/// </summary>
internal sealed class LibraryScanStagingResultsRepository : ILibraryScanStagingResultsRepository
{
    private const int INSERT_BATCH_SIZE = 500; // keeps the number of bound parameters of a single INSERT statement well under the SQLite parameter limit
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanStagingResultsRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext, used to derive the database connection string.</param>
    public LibraryScanStagingResultsRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a range of media library scan staging results to the storage medium.
    /// </summary>
    /// <param name="entities">The media library scan staging results to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertRangeAsync(IReadOnlyCollection<LibraryScanStagingResultsEntity> entities, CancellationToken cancellationToken)
    {
        // nothing to insert for an empty collection
        if (entities.Count == 0)
            return Result.Created;

        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        List<LibraryScanStagingResultsEntity> remainingEntities = [.. entities];
        // insert the entities in batches, to keep the number of bound parameters of a single INSERT statement under the SQLite parameter limit
        while (remainingEntities.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<LibraryScanStagingResultsEntity> batch = remainingEntities.GetRange(0, Math.Min(INSERT_BATCH_SIZE, remainingEntities.Count));
            await InsertBatchAsync(connection, transaction, batch, cancellationToken).ConfigureAwait(false);
            remainingEntities.RemoveRange(0, batch.Count);
        }
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Marks the media library scan staging results of the current scan by comparing them against the media library scan snapshot of a previous scan, determining which file system items
    /// are new, which ones changed and need their content hashed, and which ones are unchanged.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are marked.</param>
    /// <param name="libraryId">The unique identifier of the library whose media library scan snapshot is compared against.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Updated>> MarkChangesAgainstSnapshotAsync(Guid scanId, Guid libraryId, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        // compare the discovered file system items against the media library scan snapshot of the previous scan, entirely in the database, so that the snapshot never needs to be loaded into memory:
        // an item with no snapshot entry is new, an item with the same size and last write time is unchanged and keeps the stored hash, and everything else changed and needs its content hashed again
        const string MARK_CHANGES_SQL = """
            UPDATE LibraryScanStagingResults AS staging
            SET ContentHash = COALESCE(snapshot.ContentHash, 0),
                PreviousContentHash = COALESCE(snapshot.ContentHash, 0),
                NeedsRehash = CASE
                    WHEN snapshot.ContentHash IS NULL THEN 1
                    WHEN staging.Size = snapshot.FileSize AND staging.Ticks = snapshot.Ticks THEN 0
                    ELSE 1
                END,
                IsNew = CASE WHEN snapshot.ContentHash IS NULL THEN 1 ELSE 0 END
            FROM LibraryScanSnapshots AS snapshot
            WHERE staging.LibraryScanId = @scanId
              AND snapshot.LibraryId = @libraryId
              AND snapshot.Path = staging.Path;
            """;

        CommandDefinition command = new(MARK_CHANGES_SQL, new { scanId, libraryId }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command).ConfigureAwait(false);
        return Result.Updated;
    }

    /// <summary>
    /// Gets the number of media library scan staging results of the current scan that need their content hashed.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the number of staging results that need hashing, or an error.</returns>
    public async Task<ErrorOr<int>> GetFilesToHashCountAsync(Guid scanId, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        const string GET_FILES_TO_HASH_COUNT_SQL = """
            SELECT COUNT(*)
            FROM LibraryScanStagingResults
            WHERE LibraryScanId = @scanId
              AND NeedsRehash = 1;
            """;

        CommandDefinition command = new(GET_FILES_TO_HASH_COUNT_SQL, new { scanId }, cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<int>(command).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a page of the media library scan staging results that need their content hashed, ordered by path, using keyset pagination.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are retrieved.</param>
    /// <param name="lastPath">The path of the last retrieved file system item, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of staging results to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a page of staging results that need hashing, or an error.</returns>
    public async Task<ErrorOr<IReadOnlyList<HashedFileSystemFile>>> GetFilesToHashPageAsync(Guid scanId, string? lastPath, int pageSize, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        const string GET_FILES_TO_HASH_SQL = """
            SELECT Path, Size, Ticks, PreviousContentHash AS OldHash
            FROM LibraryScanStagingResults
            WHERE LibraryScanId = @scanId
              AND NeedsRehash = 1
              AND (@lastPath IS NULL OR Path > @lastPath)
            ORDER BY Path
            LIMIT @pageSize;
            """;

        CommandDefinition command = new(GET_FILES_TO_HASH_SQL, new { scanId, lastPath, pageSize }, cancellationToken: cancellationToken);

        IEnumerable<HashedFileSystemFile> page = await connection.QueryAsync<HashedFileSystemFile>(command).ConfigureAwait(false);
        return page.ToList();
    }

    /// <summary>
    /// Updates the content hashes of the provided media library scan staging results.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are updated.</param>
    /// <param name="hashedFiles">The file system items whose content hashes are updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Updated>> UpdateFileHashesAsync(Guid scanId, IReadOnlyCollection<HashedFileSystemFile> hashedFiles, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        const string UPDATE_HASH_SQL = """
            UPDATE LibraryScanStagingResults
            SET ContentHash = @contentHash
            WHERE LibraryScanId = @scanId
              AND Path = @path;
            """;
        foreach (HashedFileSystemFile hashedFile in hashedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CommandDefinition command = new(UPDATE_HASH_SQL, new { scanId, contentHash = hashedFile.CurrentHash, path = hashedFile.Path }, transaction, cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command).ConfigureAwait(false);
        }
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return Result.Updated;
    }

    /// <summary>
    /// Clears all the media library scan staging results of the provided media library scan from the storage medium.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are cleared.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Success>> ClearForScanAsync(Guid scanId, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        const string CLEAR_SQL = "DELETE FROM LibraryScanStagingResults WHERE LibraryScanId = @scanId;";

        CommandDefinition command = new(CLEAR_SQL, new { scanId }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command).ConfigureAwait(false);
        return Result.Success;
    }

    /// <summary>
    /// Opens a dedicated connection to the database, so that the raw SQL operations performed here do not interfere with the Entity Framework change tracking.
    /// </summary>
    /// <returns>The opened database connection.</returns>
    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        SqliteConnection connection = new(_luminaDbContext.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    /// <summary>
    /// Inserts a batch of media library scan staging results using a single multi-row INSERT statement.
    /// </summary>
    /// <param name="connection">The connection on which to execute the insert.</param>
    /// <param name="transaction">The transaction in which the insert is executed.</param>
    /// <param name="batch">The batch of media library scan staging results to insert.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private static async Task InsertBatchAsync(SqliteConnection connection, DbTransaction transaction, IReadOnlyList<LibraryScanStagingResultsEntity> batch, CancellationToken cancellationToken)
    {
        StringBuilder insertSql = new("INSERT INTO LibraryScanStagingResults (Id, LibraryScanId, Path, Size, Ticks, ContentHash, PreviousContentHash, NeedsRehash, IsNew) VALUES ");
        DynamicParameters parameters = new();
        for (int i = 0; i < batch.Count; i++)
        {
            if (i > 0)
                insertSql.Append(", ");
            insertSql.Append($"(@Id{i}, @LibraryScanId{i}, @Path{i}, @Size{i}, @Ticks{i}, @ContentHash{i}, @PreviousContentHash{i}, @NeedsRehash{i}, @IsNew{i})");
            LibraryScanStagingResultsEntity entity = batch[i];
            parameters.Add($"Id{i}", entity.Id);
            parameters.Add($"LibraryScanId{i}", entity.LibraryScanId);
            parameters.Add($"Path{i}", entity.Path);
            parameters.Add($"Size{i}", entity.Size);
            parameters.Add($"Ticks{i}", entity.Ticks);
            parameters.Add($"ContentHash{i}", entity.ContentHash);
            parameters.Add($"PreviousContentHash{i}", entity.PreviousContentHash);
            parameters.Add($"NeedsRehash{i}", entity.NeedsRehash);
            parameters.Add($"IsNew{i}", entity.IsNew);
        }

        CommandDefinition command = new(insertSql.ToString(), parameters, transaction, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }
}
