#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Tools;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Language.SetLanguage;

/// <summary>
/// API endpoint for the <c>/{culture}/tools/language/set-language</c> route.
/// </summary>
public class SetLanguageEndpoint : BaseEndpoint<SetLanguageRequest, IResult>
{
    private readonly IUrlService _urlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLanguageEndpoint"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    public SetLanguageEndpoint(IUrlService urlService)
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
        Routes(WebRoutes.Language.SET_LANGUAGE);
        DontAutoTag();
        Options(options => options.WithTags("Language"));
    }

    /// <summary>
    /// Sets the culture used by the application.
    /// </summary>
    /// <param name="request">The request containing the new culture to set and the URL to return to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SetLanguageRequest request, CancellationToken cancellationToken)
    {
        // store the culture preference in a cookie
        HttpContext.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(request.NewCulture!)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Path = "/" // ensure the cookie works across all paths
            }
        );
        // ensure the return URL includes the correct culture
        string returnUrl = string.IsNullOrEmpty(request.ReturnUrl) ? _urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED)! : request.ReturnUrl;
        // handle the culture replacement in the return URL
        string currentCulture = Culture;
        string culturePath = $"/{currentCulture.ToLower()}/";
        string newCulturePath = $"/{request.NewCulture!.ToLower()}/";

        // handle the base path if present
        if (!string.IsNullOrEmpty(HttpContext.Request.PathBase))
            returnUrl = returnUrl.Replace(HttpContext.Request.PathBase.Value!, string.Empty, StringComparison.OrdinalIgnoreCase);
        // replace the culture in the URL
        returnUrl = returnUrl.Replace(culturePath, newCulturePath, StringComparison.OrdinalIgnoreCase);

        // re-apply the base path if present
        if (!string.IsNullOrEmpty(HttpContext.Request.PathBase))
            returnUrl = HttpContext.Request.PathBase.Value + returnUrl;

        // redirect to the original page
        return Results.LocalRedirect(returnUrl);
    }
}
