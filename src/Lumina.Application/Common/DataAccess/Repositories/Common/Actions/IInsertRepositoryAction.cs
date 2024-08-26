#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Models.Common;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "insert" action for interacting with a generic persistance medium.
/// </summary>
/// <typeparam name="TModel">The type used for the insert action. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IInsertRepositoryAction<TModel> where TModel : IStorageEntity
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Saves an element of type <typeparamref name="TModel"/> in the storage medium.
    /// </summary>
    /// <param name="data">The element to be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> representing either a successfull operation, or an error.</returns>
    Task<ErrorOr<Created>> InsertAsync(TModel data, CancellationToken cancellationToken);
    #endregion
}