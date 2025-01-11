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
/// Repository for authorization user roles.
/// </summary>
internal sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public UserRoleRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new authorization user role.
    /// </summary>
    /// <param name="userRole">The authorization user role to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(UserRoleEntity userRole, CancellationToken cancellationToken)
    {
        _luminaDbContext.UserRoles.Add(userRole);
        return await Task.FromResult(Result.Created);
    }
}
