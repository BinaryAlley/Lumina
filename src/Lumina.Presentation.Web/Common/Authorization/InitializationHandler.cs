#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Polly.CircuitBreaker;
using System;
using System.Linq;
using System.Net.Http;
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
        try
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
        catch (Exception exception)
        {
            HttpContext? httpContext = GetPageNavigationDuringApiOutage(authorizationHandlerContext, exception);
            if (httpContext is null)
                throw;
            // The initialization state cannot be read while the API is unreachable, but the requested page is still rendered, and
            // the failure is reported to the user there. The data calls of the page keep failing on their own.
            httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] = true;
            authorizationHandlerContext.Succeed(initializationRequirement);
        }
    }

    /// <summary>
    /// Gets the current HTTP context when the provided exception is an API outage on a page navigation, and <see langword="null"/> otherwise.
    /// </summary>
    /// <param name="authorizationHandlerContext">The authorization context whose resource holds the HTTP context.</param>
    /// <param name="exception">The exception thrown by the initialization check against the API.</param>
    /// <returns>The HTTP context of the page navigation, or <see langword="null"/> when the exception must propagate.</returns>
    private static HttpContext? GetPageNavigationDuringApiOutage(AuthorizationHandlerContext authorizationHandlerContext, Exception exception)
    {
        if (authorizationHandlerContext.Resource is not HttpContext httpContext)
            return null;
        if (httpContext.RequestAborted.IsCancellationRequested)
            return null;
        if (exception is not ApiException and not HttpRequestException and not BrokenCircuitException and not TaskCanceledException)
            return null;
        if (exception is ApiException apiException && (int)apiException.HttpStatusCode < 500)
            return null;
        bool isPageNavigation = (httpContext.Request.Headers.Accept.Count == 0 && httpContext.Request.Headers.Range.Count == 0)
            || httpContext.Request.Headers.Accept.Any(accept => accept?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
            || string.Equals(httpContext.Request.Headers["Sec-Fetch-Mode"].ToString(), "navigate", StringComparison.OrdinalIgnoreCase)
            || string.Equals(httpContext.Request.Headers["Sec-Fetch-Dest"].ToString(), "document", StringComparison.OrdinalIgnoreCase);
        return isPageNavigation ? httpContext : null;
    }
}
