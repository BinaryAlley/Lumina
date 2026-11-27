#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Repositories.Common.Actions;
using Nemera.Application.Common.DataAccess.Repositories.Common.Base;
#endregion

namespace Nemera.Application.Common.DataAccess.Repositories.Authorization;

/// <summary>
/// Interface for the repository for authorization role permissions.
/// </summary>
public interface IRolePermissionRepository : IRepository<RolePermissionEntity>,
                                             IInsertRepositoryAction<RolePermissionEntity>
{
}
