#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
#endregion

namespace Lumina.Presentation.Web.Controllers.Tools;

/// <summary>
/// Controller for language related operations.
/// </summary>
[Route("{culture}/tools/language")]
public class LanguageController : Controller
{
    private readonly IUrlService _urlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageController"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    public LanguageController(IUrlService urlService)
    {
        _urlService = urlService;
    }

    /// <summary>
    /// Sets the culture used in the application.
    /// </summary>
    /// <param name="newCulture">The new culture to set.</param>
    /// <param name="returnUrl">The URL to return to after setting the new culture.</param>
    [HttpGet("set-language")]
    public IActionResult SetLanguage(string newCulture, string returnUrl)
    {
        // store culture preference in cookie
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(newCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Path = "/" // ensure cookie works across all paths
            }
        );
        // ensure returnUrl includes the correct culture
        if (string.IsNullOrEmpty(returnUrl))
            returnUrl = _urlService.GetAbsoluteUrl("Index", "Home")!; // default fallback URL
        // handle culture replacement in return URL
        string? currentCulture = HttpContext.Request.RouteValues["culture"]?.ToString() ?? "en-us";
        if (currentCulture != null)
        {
            // ensure we handle both with and without trailing slashes
            string culturePath = $"/{currentCulture.ToLower()}/";
            string newCulturePath = $"/{newCulture.ToLower()}/";

            // Handle base path if present
            if (!string.IsNullOrEmpty(Request.PathBase))
                returnUrl = returnUrl.Replace(Request.PathBase.Value!, "", StringComparison.OrdinalIgnoreCase);
            // replace culture in URL
            returnUrl = returnUrl.Replace(culturePath, newCulturePath, StringComparison.OrdinalIgnoreCase);

            // re-apply base path if present
            if (!string.IsNullOrEmpty(Request.PathBase))
                returnUrl = Request.PathBase.Value + returnUrl;
        }
        // redirect to the original page
        return LocalRedirect(returnUrl);
    }
}
