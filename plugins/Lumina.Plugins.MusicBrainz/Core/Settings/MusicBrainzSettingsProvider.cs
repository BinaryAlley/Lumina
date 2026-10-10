#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.Core.Settings;

/// <summary>
/// Provides the runtime settings of the MusicBrainz metadata plugin, overlaying the settings persisted by the host over the configured defaults.
/// </summary>
internal sealed class MusicBrainzSettingsProvider
{
    // The minimum request interval is clamped to a sane range, so a persisted value can neither be a non finite value that cannot be turned into a
    // duration, nor be so large that waiting for it overflows the delay of the requests.
    private const double MAXIMUM_REQUEST_INTERVAL_SECONDS = 60;

    private readonly IPluginSettingsStore? _settingsStore;
    private readonly Guid _pluginId;
    private readonly MusicBrainzSettingsDto _defaults;

    // The resolved settings are read on every request, so the resolved instance is published through Volatile to keep the common, already
    // resolved path lock free; the gate is only needed by the first callers that race to resolve it.
    private readonly SemaphoreSlim _gate = new(1, 1);
    private MusicBrainzSettingsDto? _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicBrainzSettingsProvider"/> class.
    /// </summary>
    /// <param name="settingsStore">The store of the settings persisted by the host, or <see langword="null"/> when no store is available.</param>
    /// <param name="pluginId">The unique identifier of the plugin whose settings are read.</param>
    /// <param name="defaults">The runtime settings with the default values and the optional configuration callback already applied.</param>
    public MusicBrainzSettingsProvider(IPluginSettingsStore? settingsStore, Guid pluginId, MusicBrainzSettingsDto defaults)
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
    public async Task<MusicBrainzSettingsDto> GetAsync(CancellationToken cancellationToken)
    {
        // Lock free first check: reading through Volatile guarantees that a fully resolved instance published by another thread is observed intact.
        MusicBrainzSettingsDto? runtimeSettings = Volatile.Read(ref _runtimeSettings);
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
                runtimeSettings = Clone(_defaults);
                if (_settingsStore is not null)
                {
                    IReadOnlyDictionary<string, string>? storedSettings = await _settingsStore.GetSettingsAsync(_pluginId, cancellationToken).ConfigureAwait(false);
                    if (storedSettings is not null)
                        Apply(runtimeSettings, storedSettings);
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

    /// <summary>
    /// Overlays the persisted settings onto the provided runtime settings, keeping the default values of the settings that were not persisted.
    /// </summary>
    /// <param name="settings">The runtime settings onto which the persisted settings are applied.</param>
    /// <param name="storedSettings">The settings persisted by the host, keyed by setting key.</param>
    private static void Apply(MusicBrainzSettingsDto settings, IReadOnlyDictionary<string, string> storedSettings)
    {
        // The private host opt-in must be resolved first, because it governs whether a private base URL is accepted below.
        if (storedSettings.TryGetValue(MusicBrainzSettingsKeys.DOES_ALLOW_PRIVATE_BASE_URL, out string? doesAllowPrivateBaseUrl) &&
            bool.TryParse(doesAllowPrivateBaseUrl, out bool doesAllowPrivateBaseUrlValue))
            settings.DoesAllowPrivateBaseUrl = doesAllowPrivateBaseUrlValue;

        if (storedSettings.TryGetValue(MusicBrainzSettingsKeys.BASE_URL, out string? baseUrl) &&
            Uri.TryCreate(baseUrl?.Trim(), UriKind.Absolute, out Uri? parsedBaseUrl) &&
            (parsedBaseUrl.Scheme == Uri.UriSchemeHttp || parsedBaseUrl.Scheme == Uri.UriSchemeHttps) &&
            (settings.DoesAllowPrivateBaseUrl || IsPublicHost(parsedBaseUrl.Host)))
            settings.BaseUrl = parsedBaseUrl.AbsoluteUri;

        if (storedSettings.TryGetValue(MusicBrainzSettingsKeys.CONTACT_EMAIL, out string? contactEmail))
        {
            string? trimmedContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
            settings.ContactEmail = trimmedContactEmail is not null && IsSafeContactEmail(trimmedContactEmail) ? trimmedContactEmail : null;
        }

        if (storedSettings.TryGetValue(MusicBrainzSettingsKeys.SEARCH_RESULT_LIMIT, out string? searchResultLimit) &&
            int.TryParse(searchResultLimit, NumberStyles.Integer, CultureInfo.InvariantCulture, out int searchResultLimitValue))
            settings.SearchResultLimit = Math.Max(1, searchResultLimitValue);

        if (storedSettings.TryGetValue(MusicBrainzSettingsKeys.RELEASE_LOOKUP_LIMIT, out string? releaseLookupLimit) &&
            int.TryParse(releaseLookupLimit, NumberStyles.Integer, CultureInfo.InvariantCulture, out int releaseLookupLimitValue))
            settings.ReleaseLookupLimit = Math.Max(1, releaseLookupLimitValue);

        // A persisted value that is not a finite number is rejected, and a finite one is clamped, so that neither a NaN, nor an infinity, nor an
        // out of range value can make the conversion to a duration or the waiting for it throw.
        if (storedSettings.TryGetValue(MusicBrainzSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS, out string? minimumRequestInterval) &&
            double.TryParse(minimumRequestInterval, NumberStyles.Float, CultureInfo.InvariantCulture, out double minimumRequestIntervalValue) &&
            double.IsFinite(minimumRequestIntervalValue))
            settings.MinimumRequestInterval = TimeSpan.FromSeconds(Math.Clamp(minimumRequestIntervalValue, 0, MAXIMUM_REQUEST_INTERVAL_SECONDS));
    }

    /// <summary>
    /// Determines whether the host of the persisted base URL is a public host, so the setting cannot be used to reach a local or private service.
    /// </summary>
    /// <param name="host">The host of the persisted base URL.</param>
    /// <returns><see langword="true"/> when the host is public, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A host name is accepted as is, because it cannot be classified without resolving it, which is deliberately not done here.
    /// A private or local host is accepted when <see cref="MusicBrainzSettingsDto.DoesAllowPrivateBaseUrl"/> is enabled, which supports self hosted
    /// mirrors on a local network.
    /// </remarks>
    private static bool IsPublicHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        // Uri.Host surrounds an IPv6 literal with brackets, which IPAddress.TryParse does not accept.
        string normalizedHost = host.Trim('[', ']');
        if (string.Equals(normalizedHost, "localhost", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!IPAddress.TryParse(normalizedHost, out IPAddress? address))
            return true;

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            return false;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] octets = address.GetAddressBytes();
            // The unspecified, private, loopback, link local and carrier grade NAT ranges are rejected, as they are the common targets of an SSRF.
            return octets[0] switch
            {
                0 => false,
                10 => false,
                100 when octets[1] is >= 64 and <= 127 => false,
                127 => false,
                169 when octets[1] == 254 => false,
                172 when octets[1] is >= 16 and <= 31 => false,
                192 when octets[1] == 168 => false,
                _ => true
            };
        }

        if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal)
            return false;

        byte[] addressBytes = address.GetAddressBytes();
        // The fc00::/7 unique local range is rejected, matching the private IPv4 ranges.
        if ((addressBytes[0] & 0xFE) == 0xFC)
            return false;

        return true;
    }

    /// <summary>
    /// Determines whether the persisted contact email only contains characters that are safe inside the request user agent header.
    /// </summary>
    /// <param name="contactEmail">The persisted contact email.</param>
    /// <returns><see langword="true"/> when the email is safe, otherwise <see langword="false"/>.</returns>
    private static bool IsSafeContactEmail(string contactEmail)
    {
        // The email is embedded in the User-Agent header, and an invalid character there throws when the header is parsed, which would break every request of the plugin,
        // so anything outside a conservative email charset is rejected.
        foreach (char character in contactEmail)
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('@' or '.' or '_' or '-' or '+'))
                return false;
        return true;
    }

    /// <summary>
    /// Creates a copy of the given runtime settings.
    /// </summary>
    /// <param name="source">The runtime settings to copy.</param>
    /// <returns>A copy of the given runtime settings.</returns>
    private static MusicBrainzSettingsDto Clone(MusicBrainzSettingsDto source)
    {
        return new MusicBrainzSettingsDto
        {
            BaseUrl = source.BaseUrl,
            DoesAllowPrivateBaseUrl = source.DoesAllowPrivateBaseUrl,
            UserAgent = source.UserAgent,
            ContactEmail = source.ContactEmail,
            SearchResultLimit = source.SearchResultLimit,
            ReleaseLookupLimit = source.ReleaseLookupLimit,
            MinimumRequestInterval = source.MinimumRequestInterval
        };
    }
}
