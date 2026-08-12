namespace Lumina.Plugins.OpenLibrary.Core.Settings;

/// <summary>
/// Keys of the settings of the Open Library metadata plugin, shared by the settings schema and the settings loader.
/// </summary>
internal static class OpenLibrarySettingsKeys
{
    /// <summary>
    /// The key of the setting defining the contact email sent to the Open Library API.
    /// </summary>
    internal const string CONTACT_EMAIL = "ContactEmail";

    /// <summary>
    /// The key of the setting defining the maximum number of results returned by a single search.
    /// </summary>
    internal const string SEARCH_RESULT_LIMIT = "SearchResultLimit";

    /// <summary>
    /// The key of the setting defining the maximum number of editions fetched for a single work.
    /// </summary>
    internal const string WORK_EDITION_LIMIT = "WorkEditionLimit";

    /// <summary>
    /// The key of the setting defining the minimum interval between consecutive requests to the Open Library API, in seconds.
    /// </summary>
    internal const string MINIMUM_REQUEST_INTERVAL_SECONDS = "MinimumRequestIntervalSeconds";
}
