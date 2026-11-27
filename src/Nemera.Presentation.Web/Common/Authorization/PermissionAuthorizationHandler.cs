#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization checks against the remote API.</param>
    /// <param name="httpContextAccessor">Injected accessor for the current HTTP context.</param>
    public PermissionAuthorizationHandler(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Succeeds the requirement when the currently logged in user is an administrator, or holds any of the required permissions.
    /// </summary>
    /// <param name="authorizationHandlerContext">The authorization context.</param>
    /// <param name="permissionRequirement">The permission requirement to evaluate.</param>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authorizationHandlerContext, PermissionRequirement permissionRequirement)
    {
        // The API lets administrators do what the permission allows even when they do not hold it, so the Web does not deny them what the API grants.
        if (await _authorizationService.IsInRoleAsync("Admin", CancellationToken.None).ConfigureAwait(false))
        {
            authorizationHandlerContext.Succeed(permissionRequirement);
            return;
        }
        foreach (AuthorizationPermission permission in permissionRequirement.Permissions)
        {
            if (await _authorizationService.HasPermissionAsync(permission, CancellationToken.None).ConfigureAwait(false))
            {
                authorizationHandlerContext.Succeed(permissionRequirement);
                return;
            }
        }
        // The authorization service marks a page navigation when the API could not answer. The page itself is still rendered, with
        // no permission granted, and it reports the failure to the user; the API remains the enforcement point for every data call.
        // Endpoint routing passes the Endpoint, not the HttpContext, as the authorization resource, so the current request is read
        // from the accessor instead. Only an authenticated user is allowed through, because the outage is not a reason to expose a
        // page to an anonymous visitor.
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated == true && httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] is true)
            authorizationHandlerContext.Succeed(permissionRequirement);
    }
}
