#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Deletes an element identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the element to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    Task<ErrorOr<Deleted>> DeleteByIdAsync(TId id, CancellationToken cancellationToken);
    #endregion
}