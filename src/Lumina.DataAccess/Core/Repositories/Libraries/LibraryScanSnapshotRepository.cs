#region ========================================================================= USING =====================================================================================
using Dapper;
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.DataAccess.Core.UoW;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Libraries;

/// <summary>
/// Repository for media library scan snapshots.
/// </summary>
internal sealed class LibraryScanSnapshotRepository : ILibraryScanSnapshotRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanSnapshotRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext, used to derive the database connection string.</param>
    public LibraryScanSnapshotRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets the paths of the media library scan snapshot items that are no longer present in the current scan, meaning they were deleted from the storage medium.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to get the deleted media library scan snapshot item paths.</param>
    /// <param name="scanId">The unique identifier of the media library scan for which to determine the deleted media library scan snapshot item paths.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of deleted media library scan snapshot item paths, or an error.</returns>
    public async Task<ErrorOr<IReadOnlyList<string>>> GetDeletedPathsAsync(Guid libraryId, Guid scanId, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        const string GET_DELETED_PATHS_SQL = """
            SELECT snapshot.Path
            FROM LibraryScanSnapshots AS snapshot
            WHERE snapshot.LibraryId = @libraryId
              AND NOT EXISTS (
                  SELECT 1
                  FROM LibraryScanStagingResults AS staging
                  WHERE staging.LibraryScanId = @scanId
                    AND staging.Path = snapshot.Path
              );
            """;

        CommandDefinition command = new(GET_DELETED_PATHS_SQL, new { libraryId, scanId }, cancellationToken: cancellationToken);

        IEnumerable<string> deletedPaths = await connection.QueryAsync<string>(command).ConfigureAwait(false);
        return deletedPaths.ToList();
    }

    /// <summary>
    /// Gets the paths of all the media library scan snapshot items of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library whose media library scan snapshot item paths are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of media library scan snapshot item paths, or an error.</returns>
    public async Task<ErrorOr<IReadOnlyList<string>>> GetPathsAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        const string GET_PATHS_SQL = """
            SELECT snapshot.Path
            FROM LibraryScanSnapshots AS snapshot
            WHERE snapshot.LibraryId = @libraryId;
            """;

        CommandDefinition command = new(GET_PATHS_SQL, new { libraryId }, cancellationToken: cancellationToken);

        IEnumerable<string> paths = await connection.QueryAsync<string>(command).ConfigureAwait(false);
        return paths.ToList();
    }

    /// <summary>
    /// Atomically applies the results of the current scan to the storage medium, by replacing the media library scan snapshot of the previous scan with the snapshot of the current scan.
    /// The operation updates the changed media library scan snapshot items, deletes the ones that are no longer present on disk, adds audit entries for all changed, new and deleted items,
    /// and clears the staging results of the current scan.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to apply the scan results.</param>
    /// <param name="scanId">The unique identifier of the media library scan whose results are applied.</param>
    /// <param name="userId">The unique identifier of the user that initiated the media library scan, used for audit purposes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Updated>> ApplySnapshotSwapAsync(Guid libraryId, Guid scanId, Guid userId, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        DateTime nowUtc = DateTime.UtcNow;
        object parameters = new { libraryId, scanId, userId, nowUtc };

        // the Id of the inserted rows is generated in SQL as a random 16 byte value formatted as lowercase hex (hex(randomblob(16))), which the storage layer parses as a Guid;
        // generating the Ids in code would defeat the purpose of these single statement bulk inserts

        // add an audit entry for every media library scan snapshot item that is no longer present in the current scan, meaning it was deleted from the storage medium
        const string AUDIT_DELETED_SQL = """
            INSERT INTO LibraryScanResults (Id, LibraryScanId, Status, Path, ContentHash, FileSize, Ticks)
            SELECT hex(randomblob(16)), @scanId, 'Deleted', snapshot.Path, snapshot.ContentHash, snapshot.FileSize, snapshot.Ticks
            FROM LibraryScanSnapshots AS snapshot
            WHERE snapshot.LibraryId = @libraryId
              AND NOT EXISTS (
                  SELECT 1
                  FROM LibraryScanStagingResults AS staging
                  WHERE staging.LibraryScanId = @scanId
                    AND staging.Path = snapshot.Path
              );
            """;

        CommandDefinition command = new(AUDIT_DELETED_SQL, parameters, transaction, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command).ConfigureAwait(false);

        // add an audit entry for every new or changed media library scan staging result
        const string AUDIT_CHANGED_SQL = """
            INSERT INTO LibraryScanResults (Id, LibraryScanId, Status, Path, ContentHash, FileSize, Ticks)
            SELECT hex(randomblob(16)), @scanId, CASE WHEN staging.IsNew = 1 THEN 'New' ELSE 'Modified' END, staging.Path, staging.ContentHash, staging.Size, staging.Ticks
            FROM LibraryScanStagingResults AS staging
            WHERE staging.LibraryScanId = @scanId
              AND staging.NeedsRehash = 1;
            """;

        command = new(AUDIT_CHANGED_SQL, parameters, transaction);

        await connection.ExecuteAsync(command).ConfigureAwait(false);

        // delete the media library scan snapshot items that are no longer present in the current scan, meaning they were deleted from the storage medium
        const string DELETE_MISSING_SQL = """
            DELETE FROM LibraryScanSnapshots
            WHERE LibraryId = @libraryId
              AND NOT EXISTS (
                  SELECT 1
                  FROM LibraryScanStagingResults AS staging
                  WHERE staging.LibraryScanId = @scanId
                    AND staging.Path = LibraryScanSnapshots.Path
              );
            """;

        command = new(DELETE_MISSING_SQL, parameters, transaction);

        await connection.ExecuteAsync(command).ConfigureAwait(false);

        // upsert the new and changed media library scan staging results into the media library scan snapshot
        const string UPSERT_SQL = """
            INSERT INTO LibraryScanSnapshots (Id, LibraryId, Path, ContentHash, FileSize, Ticks, CreatedOnUtc, CreatedBy, UpdatedOnUtc, UpdatedBy)
            SELECT hex(randomblob(16)), @libraryId, staging.Path, staging.ContentHash, staging.Size, staging.Ticks, @nowUtc, @userId, NULL, NULL
            FROM LibraryScanStagingResults AS staging
            WHERE staging.LibraryScanId = @scanId
              AND staging.NeedsRehash = 1
            ON CONFLICT (LibraryId, Path) DO UPDATE SET
                ContentHash = excluded.ContentHash,
                FileSize = excluded.FileSize,
                Ticks = excluded.Ticks,
                UpdatedOnUtc = @nowUtc,
                UpdatedBy = @userId;
            """;

        command = new(UPSERT_SQL, parameters, transaction);

        await connection.ExecuteAsync(command).ConfigureAwait(false);

        // clear the staging results of the current scan, as they have been fully applied to the media library scan snapshot
        const string CLEAR_STAGING_SQL = "DELETE FROM LibraryScanStagingResults WHERE LibraryScanId = @scanId;";

        command = new(CLEAR_STAGING_SQL, parameters, transaction);

        await connection.ExecuteAsync(command).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return Result.Updated;
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
}
