using Lumina.Presentation.Web.Common.Models.Configuration;
using Lumina.Presentation.Web.Common.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lumina.Presentation.Web.Common.DependencyInjection;

/// <summary>
/// Class containing the configuration bindings of the Lumina.Presentation.Web project.
/// </summary>
internal static class PresentationWebLayerConfiguration
{
    /// <summary>
    /// Extension method for registering the configuration options of the Lumina.Presentation.Web project into the Dependency Injection container.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to bind the configuration options to.</param>
    /// <param name="configuration">The configuration to be bound.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection BindConfiguration(this IServiceCollection serviceCollection, IConfigurationManager configuration)
    {
        // add the configuration sources to the configuration builder
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
        
        serviceCollection.AddOptions<ServerConfigurationModel>()
                         .Bind(configuration.GetRequiredSection(ServerConfigurationModel.SECTION_NAME))
                         .ValidateFluently()
                         .ValidateOnStart();
        return serviceCollection;
    }
}

