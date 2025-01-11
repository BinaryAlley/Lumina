#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.DataAccess.Core.UoW;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Authorization;

/// <summary>
/// Repository for authorization role permissions.
/// </summary>
internal sealed class RolePermissionRepository : IRolePermissionRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public RolePermissionRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new authorization role permission.
    /// </summary>
    /// <param name="rolePermission">The authorization role permission to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(RolePermissionEntity rolePermission, CancellationToken cancellationToken)
    {
        _luminaDbContext.RolePermissions.Add(rolePermission);
        return await Task.FromResult(Result.Created);
    }
}
