#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.LocalMusicArtwork.Common.Models.DTO.Settings;
using Lumina.Plugins.LocalMusicArtwork.Core;
using Lumina.Plugins.LocalMusicArtwork.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the local music artwork providers into the Dependency Injection container.
/// </summary>
internal static class LocalMusicArtworkServices
{
    /// <summary>
    /// Registers the services of the local music artwork providers into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the artwork.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection AddLocalMusicArtworkProviders(this IServiceCollection services, Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Build the runtime settings provider per scope, overlaying the settings persisted by the host over the defaults.
        services.TryAddScoped(serviceProvider =>
        {
            LocalMusicArtworkSettingsDto defaults = new();
            IPluginSettingsStore? settingsStore = serviceProvider.GetService<IPluginSettingsStore>();
            return new LocalMusicArtworkSettingsProvider(settingsStore, pluginId, defaults);
        });

        // Register the artwork providers as keyed by pluginId transient services, so they can be resolved specifically among other IArtworkProvider implementations.
        services.AddKeyedTransient<IArtworkProvider, LocalMusicAlbumArtworkProvider>(pluginId);
        services.AddKeyedTransient<IArtworkProvider, LocalMusicArtistArtworkProvider>(pluginId);

        return services;
    }
}
