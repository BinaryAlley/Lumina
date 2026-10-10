#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.LocalMusicArtwork.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Core.Settings;

/// <summary>
/// Provides the runtime settings of the local music artwork plugin, overlaying the settings persisted by the host over the configured defaults.
/// </summary>
internal sealed class LocalMusicArtworkSettingsProvider
{
    private readonly IPluginSettingsStore? _settingsStore;
    private readonly Guid _pluginId;
    private readonly LocalMusicArtworkSettingsDto _defaults;

    // The resolved settings are read on every request, so the resolved instance is published through Volatile to keep the common, already
    // resolved path lock free; the gate is only needed by the first callers that race to resolve it.
    private readonly SemaphoreSlim _gate = new(1, 1);
    private LocalMusicArtworkSettingsDto? _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalMusicArtworkSettingsProvider"/> class.
    /// </summary>
    /// <param name="settingsStore">The store of the settings persisted by the host, or <see langword="null"/> when no store is available.</param>
    /// <param name="pluginId">The unique identifier of the plugin whose settings are read.</param>
    /// <param name="defaults">The runtime settings with the default values and the optional configuration callback already applied.</param>
    public LocalMusicArtworkSettingsProvider(IPluginSettingsStore? settingsStore, Guid pluginId, LocalMusicArtworkSettingsDto defaults)
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
    public async Task<LocalMusicArtworkSettingsDto> GetAsync(CancellationToken cancellationToken)
    {
        // Lock free first check: reading through Volatile guarantees that a fully resolved instance published by another thread is observed intact.
        LocalMusicArtworkSettingsDto? runtimeSettings = Volatile.Read(ref _runtimeSettings);
        if (runtimeSettings is not null)
            return runtimeSettings;

        // Only the first callers reach this point, and the gate serializes them, so the settings store is queried exactly once.
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Second check inside the gate: another caller may have resolved the settings while this one was waiting for it.
            runtimeSettings = Volatile.Read(ref _runtimeSettings);
            if (runtimeSettings is null)
            {
                // The defaults are cloned so that applying the persisted settings never mutates the instance shared with the other scopes.
                runtimeSettings = new LocalMusicArtworkSettingsDto
                {
                    ShouldExtractEmbeddedCover = _defaults.ShouldExtractEmbeddedCover
                };

                if (_settingsStore is not null)
                {
                    IReadOnlyDictionary<string, string>? storedSettings = await _settingsStore.GetSettingsAsync(_pluginId, cancellationToken).ConfigureAwait(false);
                    if (storedSettings is not null &&
                        storedSettings.TryGetValue(LocalMusicArtworkSettingsKeys.SHOULD_EXTRACT_EMBEDDED_COVER, out string? shouldExtractEmbeddedCover) &&
                        bool.TryParse(shouldExtractEmbeddedCover, out bool shouldExtractEmbeddedCoverValue))
                        runtimeSettings.ShouldExtractEmbeddedCover = shouldExtractEmbeddedCoverValue;
                }

                // Publishing through Volatile makes the fully built instance visible to the lock free readers above.
                Volatile.Write(ref _runtimeSettings, runtimeSettings);
            }

            return runtimeSettings;
        }
        finally
        {
            _gate.Release();
        }
    }
}
