#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.Seed;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Seed;

/// <summary>
/// Service for seeding initial data in the persistence medium.
/// </summary>
public class DataSeedService : IDataSeedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeedService"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param
    /// <param name="dateTimeProvider">Injected service for time related concerns.</param>
    public DataSeedService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Sets up default authorization permissions in the system.
    /// </summary>
    /// <param name="adminId">The Id of the admin admin user who will own these permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Created>> SetDefaultAuthorizationPermissionsAsync(Guid adminId, CancellationToken cancellationToken)
    {
        IPermissionRepository permissionRepository = _unitOfWork.GetRepository<IPermissionRepository>();
        // create the default authorization permissions and add them to the repository.
        PermissionEntity[] defaultPermissions =
        [
            new() { PermissionName = AuthorizationPermission.CanViewUsers, CreatedBy = adminId, CreatedOnUtc = _dateTimeProvider.UtcNow },
            new() { PermissionName = AuthorizationPermission.CanDeleteUsers, CreatedBy = adminId, CreatedOnUtc = _dateTimeProvider.UtcNow },
            new() { PermissionName = AuthorizationPermission.CanRegisterUsers, CreatedBy = adminId, CreatedOnUtc = _dateTimeProvider.UtcNow },
            new() { PermissionName = AuthorizationPermission.canCreateLibraries, CreatedBy = adminId, CreatedOnUtc = _dateTimeProvider.UtcNow }
        ];
        foreach (PermissionEntity permission in defaultPermissions)
        {
            ErrorOr<Created> insertPermissionResult = await permissionRepository.InsertAsync(permission, cancellationToken).ConfigureAwait(false);
            if (insertPermissionResult.IsError)
                return insertPermissionResult.Errors;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Sets up default authorization roles in the system.
    /// </summary>
    /// <param name="userId">The Id of the admin user for whom roles will be set.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Created>> SetDefaultAuthorizationRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        IRoleRepository roleRepository = _unitOfWork.GetRepository<IRoleRepository>();

        // create the default authorization roles and add them to the repository
        RoleEntity[] defaultRoles =
        [
            new() { RoleName = "Admin", CreatedBy = userId, CreatedOnUtc = _dateTimeProvider.UtcNow }
        ];
        foreach (RoleEntity role in defaultRoles)
        {
            ErrorOr<Created> insertRoleResult = await roleRepository.InsertAsync(role, cancellationToken).ConfigureAwait(false);
            if (insertRoleResult.IsError)
                return insertRoleResult.Errors;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Assigns admin role permissions to the admin user.
    /// </summary>
    /// <param name="userId">The Id of the admin user to receive admin role permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Created>> SetAdminRolePermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        IRolePermissionRepository rolePermissionRepository = _unitOfWork.GetRepository<IRolePermissionRepository>();
        IRoleRepository roleRepository = _unitOfWork.GetRepository<IRoleRepository>();
        IPermissionRepository permissionRepository = _unitOfWork.GetRepository<IPermissionRepository>();

        // get the admin role
        ErrorOr<RoleEntity?> getAdminRoleResult = await roleRepository.GetByNameAsync("Admin", cancellationToken).ConfigureAwait(false);
        if (getAdminRoleResult.IsError)
            return getAdminRoleResult.Errors;

        if (getAdminRoleResult.Value is null)
            return Errors.Authorization.AdminAccountNotFound;

        // get all permissions
        ErrorOr<IEnumerable<PermissionEntity>> getPermissionsResult = await permissionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getPermissionsResult.IsError)
            return getPermissionsResult.Errors;

        // add each permission to the admin role
        foreach (PermissionEntity permission in getPermissionsResult.Value)
        {
            RolePermissionEntity rolePermissionEntity = new()
            {
                Permission = permission,
                PermissionId = permission.Id,
                Role = getAdminRoleResult.Value,
                RoleId = getAdminRoleResult.Value.Id,
                CreatedBy = userId,
                CreatedOnUtc = _dateTimeProvider.UtcNow
            };
            ErrorOr<Created> insertRolePermissionResult = await rolePermissionRepository.InsertAsync(rolePermissionEntity, cancellationToken).ConfigureAwait(false);
            if (insertRolePermissionResult.IsError)
                return insertRolePermissionResult.Errors;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Assigns admin role to the admin user.
    /// </summary>
    /// <param name="userId">The Id of the admin user to receive the admin role.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Created>> SetAdminRoleToAdministratorAccount(Guid userId, CancellationToken cancellationToken)
    {
        IUserRoleRepository userRoleRepository = _unitOfWork.GetRepository<IUserRoleRepository>();
        IRoleRepository roleRepository = _unitOfWork.GetRepository<IRoleRepository>();
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();


        // get the admin role
        ErrorOr<RoleEntity?> getAdminRoleResult = await roleRepository.GetByNameAsync("Admin", cancellationToken).ConfigureAwait(false);
        if (getAdminRoleResult.IsError)
            return getAdminRoleResult.Errors;

        if (getAdminRoleResult.Value is null)
            return Errors.Authorization.AdminRoleNotFound;

        // get admin user
        ErrorOr<UserEntity?> getUserResult = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsError)
            return getUserResult.Errors;

        if (getUserResult.Value is null)
            return Errors.Authorization.AdminAccountNotFound;

        // add the admin role to the admin user
        UserRoleEntity userRole = new()
        {
            CreatedBy = userId,
            CreatedOnUtc = _dateTimeProvider.UtcNow,
            Role = getAdminRoleResult.Value,
            RoleId = getAdminRoleResult.Value.Id,
            User = getUserResult.Value,
            UserId = getUserResult.Value.Id
        };
        ErrorOr<Created> insertUserRoleResult = await userRoleRepository.InsertAsync(userRole, cancellationToken).ConfigureAwait(false);
        if (insertUserRoleResult.IsError)
            return insertUserRoleResult.Errors;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }
}
