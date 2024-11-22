#region ========================================================================= USING =====================================================================================
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;

/// <summary>
/// Defines a contract for policy evaluation in the application.
/// </summary>
public interface IAuthorizationPolicy
{
    /// <summary>
    /// Evaluates a policy for a user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user for which to evaluate the policy.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the policy evaluation succeeds, <see langword="false"/> otherwise.</returns>
    Task<bool> EvaluateAsync(Guid userId, CancellationToken cancellationToken);
}
