#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Deleted>> DeleteAllAsync();
}
