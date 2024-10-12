#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Models.Common;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "get by id" action for interacting with a generic persistance medium.
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
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <typeparamref name="TModel"/> identified by <paramref name="id"/>, or an error.</returns>
    Task<ErrorOr<TModel?>> GetByIdAsync(TId id, CancellationToken cancellationToken);
}