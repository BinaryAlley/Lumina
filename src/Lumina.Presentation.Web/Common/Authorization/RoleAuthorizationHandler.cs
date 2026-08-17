#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Handles the <see cref="RoleRequirement"/> by checking the currently logged in user's role against the remote API.
/// </summary>
public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization checks against the remote API.</param>
    public RoleAuthorizationHandler(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Succeeds the requirement when the currently logged in user belongs to any of the required roles.
    /// </summary>
    /// <param name="authorizationHandlerContext">The authorization context.</param>
    /// <param name="roleRequirement">The role requirement to evaluate.</param>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authorizationHandlerContext, RoleRequirement roleRequirement)
    {
        foreach (string role in roleRequirement.Roles)
        {
            if (await _authorizationService.IsInRoleAsync(role, CancellationToken.None).ConfigureAwait(false))
            {
                authorizationHandlerContext.Succeed(roleRequirement);
                return;
            }
        }
    }
}
