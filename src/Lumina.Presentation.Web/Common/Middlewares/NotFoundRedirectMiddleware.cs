#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Middlewares;

/// <summary>
/// Middleware to handle requests towards resources that do not exist.
/// </summary>
public class NotFoundRedirectMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundRedirectMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    public NotFoundRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Handles incoming requests to resources that do not exist.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
        {
            string culture = context.Request.RouteValues["culture"]?.ToString()?.ToLower() ?? "en-us";
            string notFoundUrl = $"/{culture}/not-found";
            
            // re-execute the request so the user gets the error page
            string? originalPath = context.Request.Path.Value;
            context.Items["originalPath"] = originalPath;
            context.Request.Path = notFoundUrl;
            await _next(context);
        }
    }
}
