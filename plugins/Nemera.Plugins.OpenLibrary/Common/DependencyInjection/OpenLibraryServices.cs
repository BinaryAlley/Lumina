#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Core;
using Lumina.Plugins.OpenLibrary.Core.Api;
using Lumina.Plugins.OpenLibrary.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the Open Library metadata provider into the Dependency Injection container.
/// </summary>
internal static class OpenLibraryServices
{
    /// <summary>
    /// Registers the services of the Open Library metadata provider into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the metadata.</param>
    /// <param name="settingsCallback">Action used to configure the <see cref="OpenLibrarySettingsDto"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection AddOpenLibraryBookMetadataProvider(this IServiceCollection services, Guid pluginId, Action<OpenLibrarySettingsDto>? settingsCallback = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Build the runtime settings provider per scope, overlaying the settings persisted by the host over the defaults and the optional callback.
        services.TryAddScoped(serviceProvider =>
        {
            OpenLibrarySettingsDto defaults = new();
            settingsCallback?.Invoke(defaults);

            IPluginSettingsStore? settingsStore = serviceProvider.GetService<IPluginSettingsStore>();
            return new OpenLibrarySettingsProvider(settingsStore, pluginId, defaults);
        });

        services.AddHttpClient<OpenLibraryHttpClient>();

        // Register the metadata provider as a keyed by pluginId transient service, so it can be resolved specifically among other IMetadataProvider implementations.
        services.AddKeyedTransient<IMetadataProvider, OpenLibraryBookMetadataProvider>(pluginId);

        return services;
    }
}
