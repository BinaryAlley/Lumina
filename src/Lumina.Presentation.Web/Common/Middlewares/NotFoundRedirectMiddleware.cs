#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Middlewares;

/// <summary>
/// Middleware to handle requests towards resources that do not exist.
/// </summary>
public class NotFoundRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NotFoundRedirectMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundRedirectMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="logger">Injected service for logging.</param>
    public NotFoundRedirectMiddleware(RequestDelegate next, ILogger<NotFoundRedirectMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
            string culture = context.Request.RouteValues["culture"]?.ToString()?.ToLower() ??
                             context.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.Name?.ToLower() ??
                             "en-us";

            // re-execute the request so the user gets the error page
            string? originalPath = context.Request.Path.Value;
            context.Items["originalPath"] = originalPath;
            _logger.LogInformation("Redirecting user to NotFound url");
            context.Request.Path = $"/{culture}/not-found";
            await _next(context);
        }
    }
}
