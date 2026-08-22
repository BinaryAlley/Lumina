#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Common.Middlewares;
using Microsoft.AspNetCore.Builder;
#endregion

namespace Lumina.Presentation.Api.Common.Utilities;

/// <summary>
/// Extension methods for adding the <see cref="ExceptionHandlingMiddleware"/> to the request pipeline.
/// </summary>
public static class ApiExceptionHandlingMiddlewareUtilities
{
    /// <summary>
    /// Adds the <see cref="ExceptionHandlingMiddleware"/> to the application's request pipeline, so that any unhandled exception is converted into a ProblemDetails response.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> used to configure the application's request pipeline.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for further configuration.</returns>
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
