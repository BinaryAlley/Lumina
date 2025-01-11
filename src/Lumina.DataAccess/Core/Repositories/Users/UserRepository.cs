#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Users;

/// <summary>
/// Repository for users.
/// </summary>
internal sealed class UserRepository : IUserRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public UserRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new user.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(UserEntity user, CancellationToken cancellationToken)
    {
        bool userExists = await _luminaDbContext.Users.AnyAsync(repositoryUser => repositoryUser.Id == user.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (userExists)
            return Errors.Users.UserAlreadyExists;

        _luminaDbContext.Users.Add(user);
        return Result.Created;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="UserEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<UserEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Users.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a user identified by <paramref name="username"/> from the repository, if it exists.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="UserEntity"/> if found, or an error.</returns>
    public async Task<ErrorOr<UserEntity?>> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Users
            .Include(user => user.Libraries)
                .ThenInclude(library => library.ContentLocations)
            .Include(user => user.UserPermissions)
                .ThenInclude(userPermission => userPermission.Permission)
            .Include(user => user.UserRole!.Role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an user.
    /// </summary>
    /// <param name="data">Ther user to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Updated>> UpdateAsync(UserEntity data, CancellationToken cancellationToken)
    {
        UserEntity? foundUser = await _luminaDbContext.Users
            .Include(user => user.Libraries)
                .ThenInclude(library => library.ContentLocations)
            .Include(user => user.UserPermissions)
                .ThenInclude(userPermission => userPermission.Permission)
            .Include(user => user.UserRole!.Role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(user => user.Username == data.Username, cancellationToken)
            .ConfigureAwait(false);
        if (foundUser is null)
            return Errors.Users.UserDoesNotExist;

        // update scalar properties
        _luminaDbContext.Entry(foundUser).CurrentValues.SetValues(data);

        // update user permissions
        List<UserPermissionEntity> existingPermissions = [.. foundUser.UserPermissions];
        foreach (UserPermissionEntity permission in existingPermissions)
            _luminaDbContext.UserPermissions.Remove(permission);

        foreach (UserPermissionEntity permission in data.UserPermissions)
        {
            _luminaDbContext.UserPermissions.Add(new UserPermissionEntity
            {
                UserId = foundUser.Id,
                PermissionId = permission.PermissionId,
                Permission = permission.Permission,
                User = foundUser
            });
        }

        // update user role
        if (foundUser.UserRole != null)
            _luminaDbContext.UserRoles.Remove(foundUser.UserRole);
        if (data.UserRole is not null)
        {
            _luminaDbContext.UserRoles.Add(new UserRoleEntity
            {
                UserId = foundUser.Id,
                RoleId = data.UserRole.RoleId,
                Role = data.UserRole.Role,
                User = foundUser
            });
        }
        return Result.Updated;
    }

    /// <summary>
    /// Gets a <see cref="UserEntity"/> identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the user to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="UserEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<ErrorOr<UserEntity?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Users
            .Include(user => user.Libraries)
                .ThenInclude(library => library.ContentLocations)
            .Include(user => user.UserPermissions)
                .ThenInclude(userPermission => userPermission.Permission)
            .Include(user => user.UserRole!.Role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }
}
