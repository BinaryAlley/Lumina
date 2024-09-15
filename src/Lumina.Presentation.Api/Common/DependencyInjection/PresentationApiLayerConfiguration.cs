#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
#endregion

namespace Lumina.Presentation.Api.Common.DependencyInjection;

/// <summary>
/// Class containing the configuration bindings of the Presentation API layer.
/// </summary>
internal static class PresentationApiLayerConfiguration
{
    #region ================================================================== METHODS ===================================================================================
    /// <summary>
    /// Extension method for adding the Presentation API layer configuration to the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configuration to.</param>
    /// <param name="configuration">The configuration manager to use for the configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the base path where the configuration files should be located, does not exist.</exception>
    public static IServiceCollection BindPresentationApiLayerConfiguration(this IServiceCollection services, IConfigurationManager configuration)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"The base path '{basePath}' does not exist.");
        configuration.SetBasePath(basePath);
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

        return services;
    }
    #endregion
}