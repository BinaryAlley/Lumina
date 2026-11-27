#region ========================================================================= USING =====================================================================================
using Nemera.Plugins.Contracts.Core.Metadata;
using Nemera.Plugins.Contracts.Core.Plugins;
using Nemera.Plugins.CoverArtArchive.Common.Models.DTO.Settings;
using Nemera.Plugins.CoverArtArchive.Core;
using Nemera.Plugins.CoverArtArchive.Core.Api;
using Nemera.Plugins.CoverArtArchive.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
#endregion

namespace Nemera.Plugins.CoverArtArchive.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the Cover Art Archive artwork provider into the Dependency Injection container.
/// </summary>
internal static class CoverArtArchiveServices
{
    /// <summary>
    /// Registers the services of the Cover Art Archive artwork provider into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the artwork.</param>
    /// <param name="settingsCallback">Action used to configure the <see cref="CoverArtArchiveSettingsDto"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection AddCoverArtArchiveArtworkProvider(this IServiceCollection services, Guid pluginId, Action<CoverArtArchiveSettingsDto>? settingsCallback = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Build the runtime settings provider per scope, overlaying the settings persisted by the host over the defaults and the optional callback.
        services.TryAddScoped(serviceProvider =>
        {
            CoverArtArchiveSettingsDto defaults = new();
            settingsCallback?.Invoke(defaults);

            IPluginSettingsStore? settingsStore = serviceProvider.GetService<IPluginSettingsStore>();
            return new CoverArtArchiveSettingsProvider(settingsStore, pluginId, defaults);
        });

        // The throttle must be process-wide, because the Cover Art Archive API enforces its request policy across all callers, not per scope.
        services.TryAddSingleton<CoverArtArchiveRequestThrottle>();

        // The response cache is process-wide, so that an entity shared by many items, like the release group shared by the releases of an album,
        // is fetched only once during a run.
        services.TryAddSingleton<CoverArtArchiveResponseCache>();

        // The client follows the redirects of the Cover Art Archive itself, confined to the hosts the archive may delegate its responses to, so it opts out of the
        // host redirect handling. The host still validates and pins the address of every request and confirms that it is public.
        services.AddHttpClient<CoverArtArchiveHttpClient>();

        // Register the artwork provider as a keyed by pluginId transient service, so it can be resolved specifically among other IArtworkProvider implementations.
        services.AddKeyedTransient<IArtworkProvider, CoverArtArchiveAlbumArtworkProvider>(pluginId);

        return services;
    }
}
