#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization checks against the remote API.</param>
    /// <param name="httpContextAccessor">Injected accessor for the current HTTP context.</param>
    public RoleAuthorizationHandler(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
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
        // The authorization service marks a page navigation when the API could not answer. The page itself is still rendered, with
        // no role granted, and it reports the failure to the user; the API remains the enforcement point for every data call.
        // Endpoint routing passes the Endpoint, not the HttpContext, as the authorization resource, so the current request is read
        // from the accessor instead. Only an authenticated user is allowed through, because the outage is not a reason to expose a
        // page to an anonymous visitor.
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated == true && httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] is true)
            authorizationHandlerContext.Succeed(roleRequirement);
    }
}
