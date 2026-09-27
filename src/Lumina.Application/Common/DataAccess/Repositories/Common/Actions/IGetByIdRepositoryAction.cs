#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Common;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "get by id" action for interacting with a generic persistence medium.
/// </summary>
/// <typeparam name="TModel">The type used as a result for the "get by id" action. It should implement <see cref="IStorageEntity"/>.</typeparam>
/// <typeparam name="TId">The type used for the identifier of the respository. It should not be <see langword="null"/>.</typeparam>
public interface IGetByIdRepositoryAction<TModel, TId> where TModel : IStorageEntity
                                                       where TId : notnull
{
    /// <summary>
    /// Gets an element of type <typeparamref name="TModel"/> identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the element to get.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entity should be loaded together with the entity itself. Pass <see langword="false"/> to retrieve only the data stored directly on the entity row.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entity should be tracked by the persistence medium, so that changes to it can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <typeparamref name="TModel"/> identified by <paramref name="id"/>, or an error.</returns>
    Task<Result<TModel?>> GetByIdAsync(TId id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default);
}
