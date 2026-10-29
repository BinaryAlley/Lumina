#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Authorization.Policies.ConsumptionActivity;

/// <summary>
/// Authorization policy that checks whether a user can access the consumption activity of a user: admins can access any user's consumption activity,
/// while other users can only access their own.
/// </summary>
public class ConsumptionActivityAccessPolicy : IConsumptionActivityAccessPolicy
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumptionActivityAccessPolicy"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    public ConsumptionActivityAccessPolicy(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Evaluates the policy for the user identified by <paramref name="userId"/> against the consumption activity of the user identified in
    /// <paramref name="context"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for which to evaluate the policy.</param>
    /// <param name="context">The context carrying the Id of the user whose consumption activity is being accessed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user is an admin or accesses their own consumption activity, <see langword="false"/> otherwise.</returns>
    public async Task<bool> EvaluateAsync(Guid userId, PolicyContext? context, CancellationToken cancellationToken)
    {
        // An empty user id is a defaulted value, not a real user, so it must never match another empty id.
        if (userId == Guid.Empty)
            return false;
        if (context is not ConsumptionActivityAccessPolicyContext consumptionActivityContext)
            return false;
        if (consumptionActivityContext.TargetUserId == Guid.Empty)
            return false;
        if (consumptionActivityContext.TargetUserId == userId) // A user can access their own consumption activity.
            return true;
        // Admins can access consumption activities all the time.
        return await _authorizationService.IsInRoleAsync(userId, AuthorizationRoles.ADMIN, cancellationToken).ConfigureAwait(false);
    }
}
