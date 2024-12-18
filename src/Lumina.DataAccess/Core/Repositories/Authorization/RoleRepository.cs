#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using System;
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
        bool roleExists = await _luminaDbContext.Roles.AnyAsync(repositoryRole => repositoryRole.Id == role.Id || repositoryRole.RoleName == role.RoleName, cancellationToken).ConfigureAwait(false);
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
    public async Task<ErrorOr<RoleEntity?>> GetByNameAsync(string roleType, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(library => library.Permission)
            .FirstOrDefaultAsync(role => role.RoleName == roleType, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a <see cref="RoleEntity"/> identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the authorization role to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="RoleEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<ErrorOr<RoleEntity?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Roles
           .Include(role => role.RolePermissions)
           .ThenInclude(rolePermission => rolePermission.Permission)
           .FirstOrDefaultAsync(role => role.Id == id, cancellationToken)
           .ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a role.
    /// </summary>
    /// <param name="data">The role to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Updated>> UpdateAsync(RoleEntity data, CancellationToken cancellationToken)
    {
        // check if a role with the requested Id exists, and retrieve it
        RoleEntity? foundRole = await _luminaDbContext.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(role => role.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundRole is null)
            return Errors.Authorization.RoleNotFound;
        // update scalar properties
        _luminaDbContext.Entry(foundRole).CurrentValues.SetValues(data);
        // update owned entities (their changes are not automatically tracked by EF)
        foundRole.RolePermissions.Clear();
        foreach (RolePermissionEntity rolePermission in data.RolePermissions)
            foundRole.RolePermissions.Add(rolePermission);
        return Result.Updated;
    }

    /// <summary>
    /// Deletes a role identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the role to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // check if a role with the requested Id exists, and retrieve it
        RoleEntity? foundRole = await _luminaDbContext.Roles
            .FirstOrDefaultAsync(role => role.Id == id, cancellationToken).ConfigureAwait(false);
        if (foundRole is null)
            return Errors.Authorization.RoleNotFound;
        // then, remove it
        _luminaDbContext.Remove(foundRole);
        return Result.Deleted;
    }
}
