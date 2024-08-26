#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "delete all" action for interacting with a generic persistance medium.
/// </summary>
public interface IDeleteAllRepositoryAction
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Deletes all entities from the storage medium.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{T}"/> representing either a successfull operation, or an error.</returns>
    Task<ErrorOr<Deleted>> DeleteAllAsync();
    #endregion
}