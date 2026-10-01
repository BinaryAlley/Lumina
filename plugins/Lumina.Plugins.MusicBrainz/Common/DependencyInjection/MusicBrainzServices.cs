#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using Lumina.Plugins.MusicBrainz.Core;
using Lumina.Plugins.MusicBrainz.Core.Api;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the MusicBrainz metadata provider into the Dependency Injection container.
/// </summary>
internal static class MusicBrainzServices
{
    /// <summary>
    /// Registers the services of the MusicBrainz metadata providers into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the metadata.</param>
    /// <param name="settingsCallback">Action used to configure the <see cref="MusicBrainzSettingsDto"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection AddMusicBrainzMetadataProviders(this IServiceCollection services, Guid pluginId, Action<MusicBrainzSettingsDto>? settingsCallback = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Build the runtime settings provider per scope, overlaying the settings persisted by the host over the defaults and the optional callback.
        services.TryAddScoped(serviceProvider =>
        {
            MusicBrainzSettingsDto defaults = new();
            settingsCallback?.Invoke(defaults);

            IPluginSettingsStore? settingsStore = serviceProvider.GetService<IPluginSettingsStore>();
            return new MusicBrainzSettingsProvider(settingsStore, pluginId, defaults);
        });

        // The throttle must be process-wide, because the MusicBrainz web service enforces its request policy across all callers, not per scope.
        services.TryAddSingleton<MusicBrainzRequestThrottle>();

        // The response cache is process-wide, so that an entity shared by many items, like a release shared by all its tracks, is fetched only once.
        services.TryAddSingleton<MusicBrainzResponseCache>();

        // Redirects are not followed, so the configurable base URL cannot be used to reach an internal address through a redirect from a mirror.
        services.AddHttpClient<MusicBrainzHttpClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        // Register the metadata providers as keyed by pluginId transient services, so they can be resolved specifically among other IMetadataProvider implementations.
        // A single plugin exposes one provider per entity type, because the metadata contract is typed to a single lookup and metadata pair.
        services.AddKeyedTransient<IMetadataProvider, MusicBrainzArtistMetadataProvider>(pluginId);
        services.AddKeyedTransient<IMetadataProvider, MusicBrainzAlbumMetadataProvider>(pluginId);
        services.AddKeyedTransient<IMetadataProvider, MusicBrainzTrackMetadataProvider>(pluginId);

        return services;
    }
}
