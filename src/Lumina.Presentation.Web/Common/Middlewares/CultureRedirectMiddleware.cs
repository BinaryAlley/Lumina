#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Middlewares;

/// <summary>
/// Middleware to handle requests lacking a culture prefix in the URL, by redirecting to a default culture.
/// </summary>
public class CultureRedirectMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureRedirectMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    public CultureRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Handles incoming requests and redirects to the default culture if necessary.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // check if the request path starts with "/" and does not contain any culture
        string? path = context.Request.Path.Value?.ToLowerInvariant();
        // redirect routes that would lead to the home page
        if (path == "/" || path == "/home" || path == "/index")
        {
            // try to get the user's preferred culture from cookies
            IRequestCultureFeature? feature = context.Features.Get<IRequestCultureFeature>();
            string? userCulture = feature?.RequestCulture.Culture.Name;
            // if no specific culture is found, use "en-US" as fallback
            string redirectCulture = userCulture ?? "en-US";
            // redirect the user to their preferred culture route
            context.Response.Redirect($"/{redirectCulture.ToLower()}/");
            return;
        }
        await _next(context).ConfigureAwait(false);
    }
}
