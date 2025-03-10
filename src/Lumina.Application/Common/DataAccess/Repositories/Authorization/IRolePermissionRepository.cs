#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Authorization;

/// <summary>
/// Interface for the repository for authorization role permissions.
/// </summary>
public interface IRolePermissionRepository : IRepository<RolePermissionEntity>,
                                             IInsertRepositoryAction<RolePermissionEntity>
{
}
