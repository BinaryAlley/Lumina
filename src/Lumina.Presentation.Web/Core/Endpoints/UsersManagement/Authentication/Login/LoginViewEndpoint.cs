#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Requests.UsersManagement.Authentication;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/login/{returnUrl?}</c> route.
/// </summary>
public class LoginViewEndpoint : BaseEndpoint<LoginViewRequest, IResult>
{
    private readonly IUrlService _urlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewEndpoint"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    public LoginViewEndpoint(IUrlService urlService)
    {
        _urlService = urlService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Authentication.LOGIN_VIEW);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
        AllowAnonymous();
        PreProcessor<InitializationCheckPreProcessor<LoginViewRequest>>();
    }

    /// <summary>
    /// Displays the account login view.
    /// </summary>
    /// <param name="request">The request containing the URL to return to, after login.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(LoginViewRequest request, CancellationToken cancellationToken)
    {
        Dictionary<string, object?> viewData = new() { ["ReturnUrl"] = request.ReturnUrl };
        // if the user is already logged in, redirect them to the home page
        if (User?.Identity?.IsAuthenticated == true)
            return Task.FromResult(Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED)!));
        // check if the application is initialized (does contain at least the super user account); if it's not, redirect to registration view
        string? isPendingSuperAdminSetup = HttpContext.Session.GetString(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP);
        if (isPendingSuperAdminSetup == "true")
            return Task.FromResult(Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Authentication.REGISTER_VIEW)!));
        return Task.FromResult(View("/Core/Views/Auth/Login.cshtml", new LoginRequest(), viewData));
    }
}
