#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Middlewares;
using Microsoft.AspNetCore.Builder;
#endregion

namespace Lumina.Presentation.Web.Common.Utilities;

/// <summary>
/// Extension methods for making it easier for <see cref="ExceptionHandlingMiddleware"/> to be added the middleware.
/// </summary>
public static class ApiExceptionHandlingMiddlewareUtilities
{
    /// <summary>
    /// Adds the <see cref="ExceptionHandlingMiddleware"/> to the application's request pipeline.
    /// Handles exception raised when the server API returns a problem details.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> used to configure the application's request pipeline.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for further configuration.</returns>
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
