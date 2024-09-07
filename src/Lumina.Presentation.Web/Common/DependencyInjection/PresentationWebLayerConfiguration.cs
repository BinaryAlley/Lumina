using Lumina.Presentation.Web.Common.Models.Configuration;
using Lumina.Presentation.Web.Common.Utilities;
using Lumina.Presentation.Web.Common.Validators;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Lumina.Presentation.Web.Common.DependencyInjection;

/// <summary>
/// Class containing the configuration bindings of the Lumina.Presentation.Web project.
/// </summary>
internal static class PresentationWebLayerConfiguration
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Extension method for adding the Lumina.Presentation.Web configuration to the <see cref="WebAssemblyHostBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="WebAssemblyHostBuilder"/> to add the configuration to.</param>
    /// <returns>The updated <see cref="WebAssemblyHostBuilder"/>.</returns>
    public static WebAssemblyHostBuilder AddConfiguration(this WebAssemblyHostBuilder builder)
    {
        // add the configuration sources to the configuration builder
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

        builder.Services.BindConfiguration(builder.Configuration);

        return builder;
    }

    /// <summary>
    /// Extension method for registering the configuration options of the Lumina.Presentation.Web project into the Dependency Injection container.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to bind the configuration options to.</param>
    /// <param name="configuration">The configuration to be bound.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection BindConfiguration(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddOptions<ServerConfigurationModel>()
                         .Bind(configuration.GetRequiredSection(ServerConfigurationModel.SectionName))
                         .ValidateFluently()
                         .ValidateOnStart();
        return serviceCollection;
    }
    #endregion
}

