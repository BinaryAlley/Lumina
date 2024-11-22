#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Infrastructure.Core.Authorization.Policies.Common.Factory;
using System;
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
    /// <param name="permission">The name of the permission to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user has the specified permission, <see langword="false"/> otherwise.</returns>
    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken)
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
    /// <param name="role">The name of the role to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user is in the specified role, <see langword="false"/> otherwise.</returns>
    public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
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
}
