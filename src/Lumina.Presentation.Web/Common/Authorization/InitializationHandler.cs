#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Handles the <see cref="InitializationRequirement"/> to ensure that the application is initialized before allowing authorization.
/// </summary>
public class InitializationHandler : AuthorizationHandler<InitializationRequirement>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="InitializationHandler"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected HTTP typed client for interactions with the API.</param>
    public InitializationHandler(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Checks the application initialization status and authorizes the requirement if initialized.
    /// </summary>
    /// <param name="authorizationHandlerContext">The authorization context.</param>
    /// <param name="initializationRequirement">The initialization requirement to be evaluated.</param>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authorizationHandlerContext, InitializationRequirement initializationRequirement)
    {
        InitializationResponse result = await _apiHttpClient.GetAsync<InitializationResponse>("initialization/").ConfigureAwait(false);
        HttpContext? httpContext = authorizationHandlerContext.Resource as HttpContext;
        if (result.IsInitialized)
        {
            // admin account was registered, remove this requirement from the session
            httpContext?.Session.Remove(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP);
            authorizationHandlerContext.Succeed(initializationRequirement);
        }
        else // store in session that super admin setup is needed
            httpContext?.Session.SetString(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP, "true");
    }
}
