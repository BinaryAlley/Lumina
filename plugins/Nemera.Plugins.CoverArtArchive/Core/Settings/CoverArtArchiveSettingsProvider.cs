#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.CoverArtArchive.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Core.Settings;

/// <summary>
/// Provides the runtime settings of the Cover Art Archive artwork plugin, overlaying the settings persisted by the host over the configured defaults.
/// </summary>
internal sealed class CoverArtArchiveSettingsProvider
{
    // The minimum request interval is clamped to a sane range, so a persisted value can neither be a non finite value that cannot be turned into a
    // duration, nor be so large that waiting for it overflows the delay of the requests.
    private const double MAXIMUM_REQUEST_INTERVAL_SECONDS = 60;

    private readonly IPluginSettingsStore? _settingsStore;
    private readonly Guid _pluginId;
    private readonly CoverArtArchiveSettingsDto _defaults;

    // The resolved settings are read on every request, so the resolved instance is published through Volatile to keep the common, already
    // resolved path lock free; the gate is only needed by the first callers that race to resolve it.
    private readonly SemaphoreSlim _gate = new(1, 1);
    private CoverArtArchiveSettingsDto? _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoverArtArchiveSettingsProvider"/> class.
    /// </summary>
    /// <param name="settingsStore">The store of the settings persisted by the host, or <see langword="null"/> when no store is available.</param>
    /// <param name="pluginId">The unique identifier of the plugin whose settings are read.</param>
    /// <param name="defaults">The runtime settings with the default values and the optional configuration callback already applied.</param>
    public CoverArtArchiveSettingsProvider(IPluginSettingsStore? settingsStore, Guid pluginId, CoverArtArchiveSettingsDto defaults)
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
    public async Task<CoverArtArchiveSettingsDto> GetAsync(CancellationToken cancellationToken)
    {
        // Lock free first check: reading through Volatile guarantees that a fully resolved instance published by another thread is observed intact.
        CoverArtArchiveSettingsDto? runtimeSettings = Volatile.Read(ref _runtimeSettings);
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
                runtimeSettings = await ResolveAsync(cancellationToken).ConfigureAwait(false);
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

    /// <summary>
    /// Builds the runtime settings by overlaying the settings persisted by the host over the configured defaults.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The runtime settings of the plugin.</returns>
    private async Task<CoverArtArchiveSettingsDto> ResolveAsync(CancellationToken cancellationToken)
    {
        // The defaults are cloned so that applying the persisted settings never mutates the instance shared with the other scopes.
        CoverArtArchiveSettingsDto runtimeSettings = new()
        {
            UserAgent = _defaults.UserAgent,
            ContactEmail = _defaults.ContactEmail,
            MinimumRequestInterval = _defaults.MinimumRequestInterval
        };

        if (_settingsStore is null)
            return runtimeSettings;

        IReadOnlyDictionary<string, string>? storedSettings = await _settingsStore.GetSettingsAsync(_pluginId, cancellationToken).ConfigureAwait(false);
        if (storedSettings is null)
            return runtimeSettings;

        if (storedSettings.TryGetValue(CoverArtArchiveSettingsKeys.CONTACT_EMAIL, out string? contactEmail))
        {
            string? trimmedContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
            runtimeSettings.ContactEmail = trimmedContactEmail is not null && IsSafeContactEmail(trimmedContactEmail) ? trimmedContactEmail : null;
        }

        // A persisted value that is not a finite number is rejected, and a finite one is clamped, so that neither a NaN, nor an infinity, nor an
        // out of range value can make the conversion to a duration or the waiting for it throw.
        if (storedSettings.TryGetValue(CoverArtArchiveSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS, out string? minimumRequestInterval) &&
            double.TryParse(minimumRequestInterval, NumberStyles.Float, CultureInfo.InvariantCulture, out double minimumRequestIntervalValue) &&
            double.IsFinite(minimumRequestIntervalValue))
            runtimeSettings.MinimumRequestInterval = TimeSpan.FromSeconds(Math.Clamp(minimumRequestIntervalValue, 0, MAXIMUM_REQUEST_INTERVAL_SECONDS));

        return runtimeSettings;
    }

    /// <summary>
    /// Determines whether the persisted contact email only contains characters that are safe inside the request user agent header.
    /// </summary>
    /// <param name="contactEmail">The persisted contact email.</param>
    /// <returns><see langword="true"/> when the email is safe, otherwise <see langword="false"/>.</returns>
    private static bool IsSafeContactEmail(string contactEmail)
    {
        // The email is embedded in the User-Agent header, and an invalid character there throws when the header is parsed, which would break every request of the plugin,
        // so, anything outside a conservative email charset is rejected.
        foreach (char character in contactEmail)
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('@' or '.' or '_' or '-' or '+'))
                return false;
        return true;
    }
}
