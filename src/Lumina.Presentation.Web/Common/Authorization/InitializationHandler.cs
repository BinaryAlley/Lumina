#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Models.UsersManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Handles the <see cref="InitializationRequirement"/> to ensure that the application is initialized before allowing authorization.
/// </summary>
public class InitializationHandler : AuthorizationHandler<InitializationRequirement>
{
    private readonly IApiHttpClient _apiHttpClient;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="InitializationHandler"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected HTTP typed client for interactions with the API.</param>
    /// <param name="cache">Cache for storing the initialization status.</param>
    public InitializationHandler(IApiHttpClient apiHttpClient, IMemoryCache cache)
    {
        _apiHttpClient = apiHttpClient;
        _cache = cache;
    }

    /// <summary>
    /// Checks the application initialization status and authorizes the requirement if initialized.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The initialization requirement to be evaluated.</param>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, InitializationRequirement requirement)
    {
        InitializationModel result = await _apiHttpClient.GetAsync<InitializationModel>("initialization/").ConfigureAwait(false);
        HttpContext? httpContext = context.Resource as HttpContext;
        if (result.IsInitialized)
        {
            // admin account was registered, remove this requirement from the session
            httpContext?.Session.Remove(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP);
            context.Succeed(requirement);
        }
        else // store in session that super admin setup is needed
            httpContext?.Session.SetString(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP, "true");
    }
}
