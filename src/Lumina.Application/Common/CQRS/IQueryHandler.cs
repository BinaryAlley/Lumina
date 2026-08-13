#region ========================================================================= USING =====================================================================================
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.CQRS;

/// <summary>
/// Defines a contract for handling queries in the application layer.
/// Queries represent read-only operations that retrieve data without modifying application state.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery
{
    /// <summary>
    /// Handles the specified query and returns a result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the query execution.</returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
