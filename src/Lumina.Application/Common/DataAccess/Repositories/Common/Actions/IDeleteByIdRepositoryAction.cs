#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "delete by id" action for interacting with a generic persistance medium.
/// </summary>
/// <typeparam name="TId">The type used for the identifier of the respository. It should not be <see langword="null"/>.</typeparam>
public interface IDeleteByIdRepositoryAction<TId> where TId : notnull
{
    /// <summary>
    /// Deletes an element identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the element to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteByIdAsync(TId id, CancellationToken cancellationToken);
}
