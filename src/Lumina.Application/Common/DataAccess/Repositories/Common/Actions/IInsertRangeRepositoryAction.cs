#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "insert range" action for interacting with a generic persistance medium.
/// </summary>
/// <typeparam name="TModel">The type used for the insert range action. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IInsertRangeRepositoryAction<TModel> where TModel : IStorageEntity
{
    /// <summary>
    /// Saves a collection of elements of type <typeparamref name="TModel"/> in the storage medium.
    /// </summary>
    /// <param name="entities">The elements to be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Created>> InsertRangeAsync(IReadOnlyCollection<TModel> entities, CancellationToken cancellationToken);
}
