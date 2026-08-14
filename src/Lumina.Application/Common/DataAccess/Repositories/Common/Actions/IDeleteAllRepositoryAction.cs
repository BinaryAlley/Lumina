#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "delete all" action for interacting with a generic persistance medium.
/// </summary>
public interface IDeleteAllRepositoryAction
{
    /// <summary>
    /// Deletes all entities from the storage medium.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteAllAsync();
}
