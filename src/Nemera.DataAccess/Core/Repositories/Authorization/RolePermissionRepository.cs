#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Repositories.Authorization;
using Nemera.DataAccess.Core.UoW;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.Core.Repositories.Authorization;

/// <summary>
/// Repository for authorization role permissions.
/// </summary>
internal sealed class RolePermissionRepository : IRolePermissionRepository
{
    private readonly NemeraDbContext _nemeraDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionRepository"/> class.
    /// </summary>
    /// <param name="nemeraDbContext">Injected Entity Framework DbContext.</param>
    public RolePermissionRepository(NemeraDbContext nemeraDbContext)
    {
        _nemeraDbContext = nemeraDbContext;
    }

    /// <summary>
    /// Adds a new authorization role permission.
    /// </summary>
    /// <param name="rolePermission">The authorization role permission to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Task<Result<Created>> InsertAsync(RolePermissionEntity rolePermission, CancellationToken cancellationToken)
    {
        _nemeraDbContext.RolePermissions.Add(rolePermission);
        return Task.FromResult(Result.From(Result.Created));
    }
}
