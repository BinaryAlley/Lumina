#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Entities.Common;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "insert" action for interacting with a generic persistance medium.
/// </summary>
/// <typeparam name="TModel">The type used for the insert action. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IInsertRepositoryAction<TModel> where TModel : IStorageEntity
{
    /// <summary>
    /// Saves an element of type <typeparamref name="TModel"/> in the storage medium.
    /// </summary>
    /// <param name="data">The element to be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    Task<ErrorOr<Created>> InsertAsync(TModel data, CancellationToken cancellationToken);
}