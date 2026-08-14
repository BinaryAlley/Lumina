#region ========================================================================= USING =====================================================================================
using Dapper;
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
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
/// Repository for directory scan fingerprints.
/// </summary>
internal sealed class DirectoryScanFingerprintRepository : IDirectoryScanFingerprintRepository
{
    private const int UPSERT_BATCH_SIZE = 500; // keeps the number of bound parameters of a single UPSERT statement well under the SQLite parameter limit
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryScanFingerprintRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext, used to derive the database connection string.</param>
    public DirectoryScanFingerprintRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets the directory scan fingerprints of a media library, mapped by the directory path.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to get the directory scan fingerprints.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a path mapped dictionary of directory scan fingerprints, or an error.</returns>
    public async Task<Result<Dictionary<string, DirectoryScanFingerprintEntity>>> GetMappedByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        const string GET_FINGERPRINTS_SQL = """
            SELECT Id, LibraryId, Path, LastWriteTimeUtc
            FROM DirectoryScanFingerprints
            WHERE LibraryId = @libraryId;
            """;

        CommandDefinition command = new(GET_FINGERPRINTS_SQL, new { libraryId }, cancellationToken: cancellationToken);

        IEnumerable<DirectoryScanFingerprintEntity> fingerprints = await connection.QueryAsync<DirectoryScanFingerprintEntity>(command).ConfigureAwait(false);
        return fingerprints.ToDictionary(fingerprint => fingerprint.Path, StringComparer.Ordinal);
    }

    /// <summary>
    /// Inserts or updates the provided directory scan fingerprints in the storage medium.
    /// </summary>
    /// <param name="entities">The directory scan fingerprints to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpsertRangeAsync(IReadOnlyCollection<DirectoryScanFingerprintEntity> entities, CancellationToken cancellationToken)
    {
        // nothing to upsert for an empty collection
        if (entities.Count == 0)
            return Result.Updated;

        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        List<DirectoryScanFingerprintEntity> remainingEntities = [.. entities];
        // upsert the entities in batches, to keep the number of bound parameters of a single UPSERT statement under the SQLite parameter limit
        while (remainingEntities.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<DirectoryScanFingerprintEntity> batch = remainingEntities.GetRange(0, Math.Min(UPSERT_BATCH_SIZE, remainingEntities.Count));
            await UpsertBatchAsync(connection, transaction, batch, cancellationToken).ConfigureAwait(false);
            remainingEntities.RemoveRange(0, batch.Count);
        }
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

    /// <summary>
    /// Upserts a batch of directory scan fingerprints using a single multi-row UPSERT statement.
    /// </summary>
    /// <param name="connection">The connection on which to execute the upsert.</param>
    /// <param name="transaction">The transaction in which the upsert is executed.</param>
    /// <param name="batch">The batch of directory scan fingerprints to upsert.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private static async Task UpsertBatchAsync(SqliteConnection connection, DbTransaction transaction, IReadOnlyList<DirectoryScanFingerprintEntity> batch, CancellationToken cancellationToken)
    {
        StringBuilder upsertSql = new("INSERT INTO DirectoryScanFingerprints (Id, LibraryId, Path, LastWriteTimeUtc) VALUES ");
        DynamicParameters parameters = new();
        for (int i = 0; i < batch.Count; i++)
        {
            if (i > 0)
                upsertSql.Append(", ");
            upsertSql.Append($"(@Id{i}, @LibraryId{i}, @Path{i}, @LastWriteTimeUtc{i})");
            DirectoryScanFingerprintEntity entity = batch[i];
            parameters.Add($"Id{i}", entity.Id);
            parameters.Add($"LibraryId{i}", entity.LibraryId);
            parameters.Add($"Path{i}", entity.Path);
            parameters.Add($"LastWriteTimeUtc{i}", entity.LastWriteTimeUtc);
        }
        upsertSql.Append(" ON CONFLICT (LibraryId, Path) DO UPDATE SET LastWriteTimeUtc = excluded.LastWriteTimeUtc;");

        CommandDefinition command = new(upsertSql.ToString(), parameters, transaction, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }
}
