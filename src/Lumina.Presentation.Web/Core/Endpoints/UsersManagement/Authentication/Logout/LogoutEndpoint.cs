#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Logout;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/logout</c> route.
/// </summary>
public class LogoutEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IUrlService _urlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutEndpoint"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    public LogoutEndpoint(IUrlService urlService)
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
        Routes(WebRoutes.Authentication.LOGOUT);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
    }

    /// <summary>
    /// Logs out the account and redirects to the login page.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // if the user is not logged in, redirect them to the home page
        if (User?.Identity?.IsAuthenticated == false)
            return Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED)!);
        HttpContext.Response.Cookies.Delete("Token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        return Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Authentication.LOGIN_VIEW)!);
    }
}
