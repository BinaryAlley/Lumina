#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationPolicyFactory _authorizationPolicyFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationService"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationPolicyFactory">The factory used to generate authorization policies.</param>
    public AuthorizationService(IUnitOfWork unitOfWork, IAuthorizationPolicyFactory authorizationPolicyFactory)
    {
        _unitOfWork = unitOfWork;
        _authorizationPolicyFactory = authorizationPolicyFactory;
    }

    /// <summary>
    /// Determines whether the specified user has a specific permission.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to check the permission.</param>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user has the specified permission, <see langword="false"/> otherwise.</returns>
    public async Task<bool> HasPermissionAsync(Guid userId, AuthorizationPermission permission, CancellationToken cancellationToken)
    {
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure || getUserResult.Value is null)
            return false;

        // check if the user has the permission directly
        if (getUserResult.Value.UserPermissions.Any(userPermission => userPermission.Permission.PermissionName == permission))
            return true;

        // check if any of the user's roles grant the permission
        return getUserResult.Value.UserRole?.Role.RolePermissions
            .Any(rolePermission => rolePermission.Permission.PermissionName == permission) == true;
    }

    /// <summary>
    /// Determines whether the specified user belongs to a specific role.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to check the role.</param>
    /// <param name="role">The role to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user is in the specified role, <see langword="false"/> otherwise.</returns>
    public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
    {
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure || getUserResult.Value is null)
            return false;
        return getUserResult.Value.UserRole?.Role.RoleName == role;
    }

    /// <summary>
    /// Evaluates whether the specified user meets the conditions defined in the specified authorization policy, against the resource described by <paramref name="context"/>.
    /// </summary>
    /// <typeparam name="TAuthorizationPolicy">The type of authorization policy to evaluate.</typeparam>
    /// <param name="userId">The unique identifier of the user for whom to evaluate the policy.</param>
    /// <param name="context">The context describing the resource against which the policy is evaluated. Can be <see langword="null"/> for policies that do not require a resource.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user satisfies the policy, <see langword="false"/> otherwise.</returns>
    public async Task<bool> EvaluatePolicyAsync<TAuthorizationPolicy>(Guid userId, PolicyContext? context, CancellationToken cancellationToken) where TAuthorizationPolicy : IAuthorizationPolicy
    {
        // resolve the authorization policy dynamically using the factory
        IAuthorizationPolicy policy = _authorizationPolicyFactory.CreatePolicy<TAuthorizationPolicy>();
        return await policy.EvaluateAsync(userId, context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all authorization roles and permissions of a user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The id of the user for whom to retrieve the authorization roles and permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="UserAuthorizationEntity"/>, or an error.</returns>
    public async Task<Result<UserAuthorizationEntity>> GetUserAuthorizationAsync(Guid userId, CancellationToken cancellationToken)
    {
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);

        if (getUserResult.IsFailure)
            return getUserResult.Errors;

        if (getUserResult.Value is null)
            return Errors.Users.UserDoesNotExist;

        // get all roles
        string? role = getUserResult.Value.UserRole?.Role.RoleName;

        // get direct user permissions
        HashSet<AuthorizationPermission> directPermissions = [.. getUserResult.Value.UserPermissions.Select(userPermission => userPermission.Permission.PermissionName)];

        // get permissions from roles
        HashSet<AuthorizationPermission>? rolePermissions = getUserResult.Value.UserRole?.Role.RolePermissions
            .Select(rolePermission => rolePermission.Permission.PermissionName)
            .ToHashSet();

        // combine all permissions
        HashSet<AuthorizationPermission> allPermissions = rolePermissions is not null ? [.. directPermissions.Union(rolePermissions)] : directPermissions;

        return new UserAuthorizationEntity
        {
            UserId = userId,
            Role = role,
            Permissions = allPermissions
        };
    }
}
