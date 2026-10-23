#region ========================================================================= USING =====================================================================================
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Common.Interceptors;

/// <summary>
/// Registers the SQL function used by the shared reference normalization migration, so that names are folded with the same culture-invariant rule as the domain value objects.
/// </summary>
public sealed class SqliteSharedReferenceNormalizationInterceptor : DbConnectionInterceptor
{
    /// <summary>
    /// The name of the SQL function that trims a name and lowercases it with the invariant culture.
    /// </summary>
    public const string NORMALIZE_FUNCTION_NAME = "lumina_normalize_shared_reference_name";

    /// <summary>
    /// Intercepts a synchronously opened connection to register the normalization function.
    /// </summary>
    /// <param name="connection">The connection that was opened.</param>
    /// <param name="eventData">The event data of the opened connection.</param>
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        RegisterNormalizationFunction(connection);
        base.ConnectionOpened(connection, eventData);
    }

    /// <summary>
    /// Intercepts an asynchronously opened connection to register the normalization function.
    /// </summary>
    /// <param name="connection">The connection that was opened.</param>
    /// <param name="eventData">The event data of the opened connection.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        RegisterNormalizationFunction(connection);
        return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    /// <summary>
    /// Registers the normalization function on <paramref name="connection"/> when it is a SQLite connection.
    /// </summary>
    /// <param name="connection">The connection that was opened.</param>
    private static void RegisterNormalizationFunction(DbConnection connection)
    {
        if (connection is not SqliteConnection sqliteConnection)
            return;
        sqliteConnection.CreateFunction(NORMALIZE_FUNCTION_NAME, (string? name) => name?.Trim().ToLowerInvariant(), isDeterministic: true);
    }
}
