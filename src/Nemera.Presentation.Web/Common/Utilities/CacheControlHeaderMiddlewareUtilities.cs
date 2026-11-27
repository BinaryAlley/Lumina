#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Middlewares;
using Microsoft.AspNetCore.Builder;
#endregion

namespace Lumina.Presentation.Web.Common.Utilities;

/// <summary>
/// Extension methods for adding the <see cref="CacheControlHeaderMiddleware"/> to the request pipeline.
/// </summary>
public static class CacheControlHeaderMiddlewareUtilities
{
    /// <summary>
    /// Adds the <see cref="CacheControlHeaderMiddleware"/> to the application's request pipeline.
    /// </summary>
    /// <remarks>
    /// It must be registered before the session middleware, so that its response-start callback runs after the one of the session middleware.
    /// </remarks>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> used to configure the application's request pipeline.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for further configuration.</returns>
    public static IApplicationBuilder UseCacheControlHeader(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CacheControlHeaderMiddleware>();
    }
}
