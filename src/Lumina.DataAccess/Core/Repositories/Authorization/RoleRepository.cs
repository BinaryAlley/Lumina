#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Enums.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Authorization;

/// <summary>
/// Repository for authorization roles.
/// </summary>
internal sealed class RoleRepository : IRoleRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public RoleRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new authorization role.
    /// </summary>
    /// <param name="role">The authorization role to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        bool roleExists = await _luminaDbContext.Roles.AnyAsync(repositoryRole => repositoryRole.Id == role.Id, cancellationToken).ConfigureAwait(false);
        if (roleExists)
            return Errors.Authorization.RoleAlreadyExists;

        _luminaDbContext.Roles.Add(role);
        return Result.Created;
    }

    /// <summary>
    /// Gets all autorization roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="RoleEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<RoleEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Roles.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a role identified by <paramref name="roleType"/> from the repository, if it exists.
    /// </summary>
    /// <param name="roleType">The type of role to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="RoleEntity"/> if found, or an error.</returns>
    public async Task<ErrorOr<RoleEntity?>> GetByNameAsync(AuthorizationRole roleType, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(library => library.Permission)
            .FirstOrDefaultAsync(role => role.RoleName == roleType, cancellationToken)
            .ConfigureAwait(false);
    }
}
