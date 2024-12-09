#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Models.Authorization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Interface for the service for managing authorization.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Determines whether the currently logged in user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the currently logged in user has the specified permission, <see langword="false"/> otherwise.</returns>
    Task<bool> HasPermissionAsync(AuthorizationPermission permission, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the currently logged in user belongs to a specific role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the currently logged in user is in the specified role, <see langword="false"/> otherwise.</returns>
    Task<bool> IsInRoleAsync(AuthorizationRole role, CancellationToken cancellationToken);

    /// <summary>
    /// Evaluates whether the currently logged in user meets the conditions defined in the specified authorization policy.
    /// </summary>
    /// <typeparam name="TAuthorizationPolicy">The type of authorization policy to evaluate.</typeparam>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the currently logged in user satisfies the policy, <see langword="false"/> otherwise.</returns>
    Task<bool> EvaluatePolicyAsync<TAuthorizationPolicy>(CancellationToken cancellationToken);

    /// <summary>
    /// Evaluates whether the currently logged in user meets the conditions defined in the specified authorization requirement.
    /// </summary>
    /// <param name="requirement">The authorization requirements to check.</param>
    /// <returns><see langword="true"/> if the currently logged in user satisfies the requirement, <see langword="false"/> otherwise.</returns>
    Task<bool> EvaluateAuthorizationAsync(AuthorizationRequirementModel requirement);
}
