#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "get all" action for interacting with a generic persistance medium.
/// </summary>
/// <typeparam name="TModel">The type used as a result for the "get all" action. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IGetAllRepositoryAction<TModel> where TModel : IStorageEntity
{
    /// <summary>
    /// Gets all data of type <typeparamref name="TModel"/> from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <typeparamref name="TModel"/>, or an error.</returns>
    Task<ErrorOr<IEnumerable<TModel>>> GetAllAsync(CancellationToken cancellationToken);
}
