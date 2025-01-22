#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Common;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "update" action for interacting with a generic persistance medium.
/// </summary>
/// <typeparam name="TModel">The type used for the update action. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IUpdateRepositoryAction<TModel> where TModel : IStorageEntity
{
    /// <summary>
    /// Updates <paramref name="data"/> in the storage medium.
    /// </summary>
    /// <param name="data">The element that will be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Updated>> UpdateAsync(TModel data, CancellationToken cancellationToken);
}
