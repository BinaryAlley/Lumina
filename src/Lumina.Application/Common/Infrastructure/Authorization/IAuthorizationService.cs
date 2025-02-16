#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Infrastructure.Authorization;

/// <summary>
/// Interface for the service for managing authorization.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Determines whether the specified user has a specific permission.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to check the permission.</param>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user has the specified permission, <see langword="false"/> otherwise.</returns>
    Task<bool> HasPermissionAsync(Guid userId, AuthorizationPermission permission, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the specified user belongs to a specific role.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to check the role.</param>
    /// <param name="role">The role to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user is in the specified role, <see langword="false"/> otherwise.</returns>
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken);

    /// <summary>
    /// Evaluates whether the specified user meets the conditions defined in the specified authorization policy.
    /// </summary>
    /// <typeparam name="TAuthorizationPolicy">The type of authorization policy to evaluate.</typeparam>
    /// <param name="userId">The unique identifier of the user for whom to evaluate the policy.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user satisfies the policy, <see langword="false"/> otherwise.</returns>
    Task<bool> EvaluatePolicyAsync<TAuthorizationPolicy>(Guid userId, CancellationToken cancellationToken) where TAuthorizationPolicy : IAuthorizationPolicy;

    /// <summary>
    /// Retrieves all authorization roles and permissions of a user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to retrieve the authorization roles and permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="UserAuthorizationEntity"/>, or an error.</returns>
    Task<ErrorOr<UserAuthorizationEntity>> GetUserAuthorizationAsync(Guid userId, CancellationToken cancellationToken);
}
