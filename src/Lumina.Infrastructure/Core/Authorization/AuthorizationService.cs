#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Domain.Common.Errors;
using Lumina.Infrastructure.Core.Authorization.Policies.Common.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Authorization;

/// <summary>
/// Service for managing authorization.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthorizationPolicyFactory _authorizationPolicyFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationService"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationPolicyFactory">The factory used to generate authorization policies.</param>
    public AuthorizationService(IUnitOfWork unitOfWork, IAuthorizationPolicyFactory authorizationPolicyFactory)
    {
        _userRepository = unitOfWork.GetRepository<IUserRepository>();
        _authorizationPolicyFactory = authorizationPolicyFactory;
    }

    /// <summary>
    /// Determines whether the specified user has a specific permission.
    /// </summary>
    /// <param name="userId">The Id of the user for whom to check the permission.</param>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user has the specified permission, <see langword="false"/> otherwise.</returns>
    public async Task<bool> HasPermissionAsync(Guid userId, AuthorizationPermission permission, CancellationToken cancellationToken)
    {
        ErrorOr<UserEntity?> getUserResult = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsError || getUserResult.Value is null)
            return false;

        // check if the user has the permission directly
        if (getUserResult.Value.UserPermissions.Any(userPermission => userPermission.Permission.PermissionName == permission))
            return true;

        // check if any of the user's roles grant the permission
        return getUserResult.Value.UserRoles
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .Any(rolePermission => rolePermission.Permission.PermissionName == permission);
    }

    /// <summary>
    /// Determines whether the specified user belongs to a specific role.
    /// </summary>
    /// <param name="userId">The Id of the user for whom to check the role.</param>
    /// <param name="role">The role to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user is in the specified role, <see langword="false"/> otherwise.</returns>
    public async Task<bool> IsInRoleAsync(Guid userId, AuthorizationRole role, CancellationToken cancellationToken)
    {
        ErrorOr<UserEntity?> getUserResult = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsError || getUserResult.Value is null)
            return false;
        return getUserResult.Value.UserRoles.Any(userRole => userRole.Role.RoleName == role);
    }

    /// <summary>
    /// Evaluates whether the specified user meets the conditions defined in the specified authorization policy.
    /// </summary>
    /// <typeparam name="TAuthorizationPolicy">The type of authorization policy to evaluate.</typeparam>
    /// <param name="userId">The Id of the user for whom to evaluate the policy.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user satisfies the policy, <see langword="false"/> otherwise.</returns>
    public async Task<bool> EvaluatePolicyAsync<TAuthorizationPolicy>(Guid userId, CancellationToken cancellationToken) where TAuthorizationPolicy : IAuthorizationPolicy
    {
        // resolve the authorization policy dynamically using the factory
        IAuthorizationPolicy policy = _authorizationPolicyFactory.CreatePolicy<TAuthorizationPolicy>();
        return await policy.EvaluateAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all authorization roles and permissions of a user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The id of the user for whom to retrieve the authorization roles and permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="UserAuthorizationEntity"/>, or an error.</returns>
    public async Task<ErrorOr<UserAuthorizationEntity>> GetUserAuthorizationAsync(Guid userId, CancellationToken cancellationToken)
    {
        ErrorOr<UserEntity?> getUserResult = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);

        if (getUserResult.IsError)
            return getUserResult.Errors;

        if (getUserResult.Value is null)
            return Errors.Users.UserDoesNotExist;

        // get all roles
        HashSet<AuthorizationRole> roles = getUserResult.Value.UserRoles
            .Select(userRole => userRole.Role.RoleName)
            .ToHashSet();

        // get direct user permissions
        HashSet<AuthorizationPermission> directPermissions = getUserResult.Value.UserPermissions
            .Select(userPermission => userPermission.Permission.PermissionName)
            .ToHashSet();

        // get permissions from roles
        HashSet<AuthorizationPermission> rolePermissions = getUserResult.Value.UserRoles
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.PermissionName)
            .ToHashSet();

        // combine all permissions
        HashSet<AuthorizationPermission> allPermissions = directPermissions
            .Union(rolePermissions)
            .ToHashSet();

        return new UserAuthorizationEntity
        {
            UserId = userId,
            Roles = roles,
            Permissions = allPermissions
        };
    }
}
