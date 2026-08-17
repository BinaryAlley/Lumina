#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Handles the <see cref="PermissionRequirement"/> by checking the currently logged in user's permissions against the remote API.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization checks against the remote API.</param>
    public PermissionAuthorizationHandler(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Succeeds the requirement when the currently logged in user holds any of the required permissions.
    /// </summary>
    /// <param name="authorizationHandlerContext">The authorization context.</param>
    /// <param name="permissionRequirement">The permission requirement to evaluate.</param>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authorizationHandlerContext, PermissionRequirement permissionRequirement)
    {
        foreach (AuthorizationPermission permission in permissionRequirement.Permissions)
        {
            if (await _authorizationService.HasPermissionAsync(permission, CancellationToken.None).ConfigureAwait(false))
            {
                authorizationHandlerContext.Succeed(permissionRequirement);
                return;
            }
        }
    }
}
