namespace Lumina.Plugins.MusicBrainz.Core.Settings;

/// <summary>
/// Keys of the settings of the MusicBrainz metadata plugin, shared by the settings schema and the settings loader.
/// </summary>
internal static class MusicBrainzSettingsKeys
{
    /// <summary>
    /// The key of the setting defining the base URL of the MusicBrainz web service.
    /// </summary>
    internal const string BASE_URL = "BaseUrl";

    /// <summary>
    /// The key of the setting that allows the base URL to point at a private or local host, for a self hosted mirror on a local network.
    /// </summary>
    internal const string DOES_ALLOW_PRIVATE_BASE_URL = "DoesAllowPrivateBaseUrl";

    /// <summary>
    /// The key of the setting defining the contact email sent to the MusicBrainz API.
    /// </summary>
    internal const string CONTACT_EMAIL = "ContactEmail";

    /// <summary>
    /// The key of the setting defining the maximum number of results returned by a single search.
    /// </summary>
    internal const string SEARCH_RESULT_LIMIT = "SearchResultLimit";

    /// <summary>
    /// The key of the setting defining the maximum number of releases fetched for a single release group.
    /// </summary>
    internal const string RELEASE_LOOKUP_LIMIT = "ReleaseLookupLimit";

    /// <summary>
    /// The key of the setting defining the minimum interval between consecutive requests to the MusicBrainz API, in seconds.
    /// </summary>
    internal const string MINIMUM_REQUEST_INTERVAL_SECONDS = "MinimumRequestIntervalSeconds";
}
