#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.Common.Primitives;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "apply update" action for interacting with a generic persistence medium.
/// </summary>
/// <typeparam name="TModel">The type used for the apply update action. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IApplyUpdateRepositoryAction<TModel> where TModel : IStorageEntity
{
    /// <summary>
    /// Applies the editable values of <paramref name="data"/> onto the already tracked <paramref name="tracked"/> model, without loading it again.
    /// </summary>
    /// <param name="tracked">The tracked model whose editable values are updated.</param>
    /// <param name="data">The model carrying the desired values.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ApplyUpdateAsync(TModel tracked, TModel data, CancellationToken cancellationToken);
}
