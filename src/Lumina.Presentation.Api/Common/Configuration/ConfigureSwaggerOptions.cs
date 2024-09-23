#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
#endregion

namespace Lumina.Presentation.Api.Common.Configuration;

/// <summary>
/// Configures the Swagger options.
/// </summary>
public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IApiVersionDescriptionProvider _provider;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
    /// </summary>
    /// <param name="provider">Provider for API version descriptions.</param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Configures the Swagger options.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    public void Configure(SwaggerGenOptions options)
    {
        // add swagger document for every API version discovered
        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
    }

    /// <summary>
    /// Configures the Swagger options.
    /// </summary>
    /// <param name="name">The name of the options instance being configured.</param>
    /// <param name="options">The options instance to configure.</param>
    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    /// <summary>
    /// Creates the version information for the Swagger document.
    /// </summary>
    /// <param name="description">The API version description.</param>
    /// <returns>The created <see cref="OpenApiInfo"/> instance.</returns>
    private OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        OpenApiInfo info = new()
        {
            Title = "Lumina API",
            Version = description.ApiVersion.ToString()
        };
        if (description.IsDeprecated)
            info.Description += " This API version has been deprecated.";
        return info;
    }
    #endregion
}