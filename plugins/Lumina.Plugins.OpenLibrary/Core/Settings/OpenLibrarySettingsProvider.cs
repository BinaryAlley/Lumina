#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.OpenLibrary.Core.Settings;

/// <summary>
/// Provides the runtime settings of the Open Library metadata plugin, overlaying the settings persisted by the host over the configured defaults.
/// </summary>
internal sealed class OpenLibrarySettingsProvider
{
    private readonly IPluginSettingsStore? _settingsStore;
    private readonly Guid _pluginId;
    private readonly OpenLibrarySettingsDto _defaults;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private OpenLibrarySettingsDto? _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenLibrarySettingsProvider"/> class.
    /// </summary>
    /// <param name="settingsStore">The store of the settings persisted by the host, or <see langword="null"/> when no store is available.</param>
    /// <param name="pluginId">The unique identifier of the plugin whose settings are read.</param>
    /// <param name="defaults">The runtime settings with the default values and the optional configuration callback already applied.</param>
    public OpenLibrarySettingsProvider(IPluginSettingsStore? settingsStore, Guid pluginId, OpenLibrarySettingsDto defaults)
    {
        _settingsStore = settingsStore;
        _pluginId = pluginId;
        _defaults = defaults;
    }

    /// <summary>
    /// Gets the runtime settings of the plugin, reading and applying the settings persisted by the host on the first call.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The runtime settings of the plugin.</returns>
    public async Task<OpenLibrarySettingsDto> GetAsync(CancellationToken cancellationToken)
    {
        OpenLibrarySettingsDto? runtimeSettings = Volatile.Read(ref _runtimeSettings);
        if (runtimeSettings is not null)
            return runtimeSettings;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            runtimeSettings = Volatile.Read(ref _runtimeSettings);
            if (runtimeSettings is null)
            {
                runtimeSettings = Clone(_defaults);
                if (_settingsStore is not null)
                {
                    IReadOnlyDictionary<string, string>? storedSettings = await _settingsStore.GetSettingsAsync(_pluginId, cancellationToken).ConfigureAwait(false);
                    if (storedSettings is not null)
                        OpenLibrarySettingsLoader.Apply(runtimeSettings, storedSettings);
                }

                Volatile.Write(ref _runtimeSettings, runtimeSettings);
            }

            return runtimeSettings;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Creates a copy of the given runtime settings.
    /// </summary>
    /// <param name="source">The runtime settings to copy.</param>
    /// <returns>A copy of the given runtime settings.</returns>
    private static OpenLibrarySettingsDto Clone(OpenLibrarySettingsDto source)
    {
        return new OpenLibrarySettingsDto
        {
            UserAgent = source.UserAgent,
            ContactEmail = source.ContactEmail,
            SearchResultLimit = source.SearchResultLimit,
            WorkEditionLimit = source.WorkEditionLimit,
            MinimumRequestInterval = source.MinimumRequestInterval
        };
    }
}
