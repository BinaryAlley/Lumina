#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
#endregion

namespace Lumina.Presentation.Web.Common.MiddlewareFilters;

/// <summary>
/// Defines a pipeline for configuring middleware to handle localization.
/// </summary>
/// <remarks>
/// To use a MiddlewareFilter, you first need to create a pipeline. This acts as a mini startup configuration where you define an <see cref="IApplicationBuilder"/>
/// to specify the middleware that should run as part of the pipeline. Multiple middleware can be configured in this way.
/// </remarks>
public class LocalizationPipeline
{
    /// <summary>
    /// Configures the middleware pipeline to include the <see cref="RequestLocalizationMiddleware"/> for handling localization.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> used to configure the middleware pipeline.</param>
    /// <param name="options">The <see cref="IOptions{TOptions}"/> instance for <see cref="RequestLocalizationOptions"/>, containing localization settings to be applied.</param>
    public void Configure(IApplicationBuilder app, IOptions<RequestLocalizationOptions> options)
    {
        // runs the RequestLocalizationMiddleware with the provided options
        app.UseRequestLocalization(options.Value);
    }
}
