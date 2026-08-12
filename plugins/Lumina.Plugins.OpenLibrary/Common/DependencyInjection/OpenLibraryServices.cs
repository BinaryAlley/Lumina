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
using System.Collections.Generic;
using System.Threading;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.DependencyInjection;

/// <summary>
/// Provides extension methods for registering the Open Library metadata provider services.
/// </summary>
internal static class OpenLibraryServices
{
    /// <summary>
    /// Registers the Open Library book metadata provider and its dependencies, associated with the plugin that provides it.
    /// </summary>
    /// <param name="services">The service collection to register the services into.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the metadata.</param>
    /// <param name="settingsCallback">Action used to configure the <see cref="OpenLibrarySettingsDto"/>.</param>
    /// <returns>The service collection, for chaining.</returns>
    internal static IServiceCollection AddOpenLibraryBookMetadataProvider(this IServiceCollection services, Guid pluginId, Action<OpenLibrarySettingsDto>? settingsCallback = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Build the runtime settings per scope, overlaying the settings persisted by the host over the defaults and the optional callback.
        services.TryAddScoped(serviceProvider =>
        {
            OpenLibrarySettingsDto openLibrarySettings = new();
            settingsCallback?.Invoke(openLibrarySettings);

            IPluginSettingsStore? settingsStore = serviceProvider.GetService<IPluginSettingsStore>();
            if (settingsStore is not null)
            {
                IReadOnlyDictionary<string, string>? storedSettings = settingsStore.GetSettingsAsync(pluginId, CancellationToken.None).GetAwaiter().GetResult();
                if (storedSettings is not null)
                    OpenLibrarySettingsLoader.Apply(openLibrarySettings, storedSettings);
            }

            return openLibrarySettings;
        });

        services.AddHttpClient<OpenLibraryHttpClient>();

        // Register the metadata provider as a keyed by pluginId transient service, so it can be resolved specifically among other IRemoteMetadataProvider implementations.
        services.AddKeyedTransient<IRemoteMetadataProvider, OpenLibraryBookMetadataProvider>(pluginId);

        return services;
    }
}
