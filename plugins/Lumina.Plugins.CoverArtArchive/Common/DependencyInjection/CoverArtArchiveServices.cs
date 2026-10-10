#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.CoverArtArchive.Common.Models.DTO.Settings;
using Lumina.Plugins.CoverArtArchive.Core;
using Lumina.Plugins.CoverArtArchive.Core.Api;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Common.DependencyInjection;

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

        // Automatic redirects are disabled on purpose: the metadata endpoints answer with a temporary redirect to the Internet Archive, and the client
        // follows that redirect itself, checking every hop against CoverArtArchiveHosts. Letting the primary handler follow redirects automatically
        // would make it follow whatever Location the response carries, which is server controlled, and would bypass that host check, which is the SSRF
        // control here; the base URL being a constant does not make the redirect target trustworthy.
        services.AddHttpClient<CoverArtArchiveHttpClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        // Register the artwork provider as a keyed by pluginId transient service, so it can be resolved specifically among other IArtworkProvider implementations.
        services.AddKeyedTransient<IArtworkProvider, CoverArtArchiveAlbumArtworkProvider>(pluginId);

        return services;
    }
}
