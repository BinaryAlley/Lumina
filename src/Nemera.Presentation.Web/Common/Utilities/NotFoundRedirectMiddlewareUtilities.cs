#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Middlewares;
using Microsoft.AspNetCore.Builder;
#endregion

namespace Lumina.Presentation.Web.Common.Utilities;

/// <summary>
/// Extension methods for making it easier for <see cref="NotFoundRedirectMiddleware"/> to be added the middleware.
/// </summary>
public static class NotFoundRedirectMiddlewareUtilities
{
    /// <summary>
    /// Adds the <see cref="NotFoundRedirectMiddleware"/> to the application's request pipeline.
    /// Redirects requests to resources that do not exist.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> used to configure the application's request pipeline.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for further configuration.</returns>
    public static IApplicationBuilder UseNotFoundRedirect(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NotFoundRedirectMiddleware>();
    }
}
