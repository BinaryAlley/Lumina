#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Middlewares;
using Microsoft.AspNetCore.Builder;
#endregion

namespace Lumina.Presentation.Web.Common.Utilities;

/// <summary>
/// Extension methods for making it easier for <see cref="CultureRedirectMiddleware"/> to be added the middleware.
/// </summary>
// Extension method to make it easier to add the middleware
public static class CultureRedirectMiddlewareExtensions
{
    /// <summary>
    /// Adds the <see cref="CultureRedirectMiddleware"/> to the application's request pipeline.
    /// Redirects requests to include or validate the culture in the URL.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> used to configure the application's request pipeline.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for further configuration.</returns>
    public static IApplicationBuilder UseCultureRedirect(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CultureRedirectMiddleware>();
    }
}
