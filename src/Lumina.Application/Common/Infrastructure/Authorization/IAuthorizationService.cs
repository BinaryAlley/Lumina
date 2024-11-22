#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
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
    /// <param name="userId">The Id of the user for whom to check the permission.</param>
    /// <param name="permission">The name of the permission to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user has the specified permission, <see langword="false"/> otherwise.</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the specified user belongs to a specific role.
    /// </summary>
    /// <param name="userId">The Id of the user for whom to check the role.</param>
    /// <param name="role">The name of the role to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user is in the specified role, <see langword="false"/> otherwise.</returns>
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken);

    /// <summary>
    /// Evaluates whether the specified user meets the conditions defined in the specified authorization policy.
    /// </summary>
    /// <typeparam name="TAuthorizationPolicy">The type of authorization policy to evaluate.</typeparam>
    /// <param name="userId">The Id of the user for whom to evaluate the policy.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user satisfies the policy, <see langword="false"/> otherwise.</returns>
    Task<bool> EvaluatePolicyAsync<TAuthorizationPolicy>(Guid userId, CancellationToken cancellationToken) where TAuthorizationPolicy : IAuthorizationPolicy;
}
