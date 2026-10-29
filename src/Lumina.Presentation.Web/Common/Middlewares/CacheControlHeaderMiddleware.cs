#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Http;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Middlewares;

/// <summary>
/// Middleware that restores the <c>Cache-Control</c> header chosen by an endpoint after any middleware that overwrites it.
/// </summary>
/// <remarks>
/// The session middleware sets <c>Cache-Control: no-cache,no-store</c>, <c>Pragma: no-cache</c> and <c>Expires: -1</c> when it establishes
/// a new session, and it registers that write before the endpoint runs. Response-start callbacks run in reverse registration order, so a
/// callback registered here, before the session middleware, runs after the session middleware and can restore the endpoint's value.
/// </remarks>
public class CacheControlHeaderMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheControlHeaderMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    public CacheControlHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Restores the endpoint's <c>Cache-Control</c> value when the response starts, after the response-start callbacks registered later, such as the session middleware's, have run.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            if (context.Items.TryGetValue(HttpContextItemKeys.CACHE_CONTROL, out object? value) && value is string cacheControl && !string.IsNullOrEmpty(cacheControl))
                context.Response.Headers.CacheControl = cacheControl;
            return Task.CompletedTask;
        });
        await _next(context);
    }
}
