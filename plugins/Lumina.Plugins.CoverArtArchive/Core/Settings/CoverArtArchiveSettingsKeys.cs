namespace Lumina.Plugins.CoverArtArchive.Core.Settings;

/// <summary>
/// Keys of the settings of the Cover Art Archive artwork plugin, shared by the settings schema and the settings loader.
/// </summary>
internal static class CoverArtArchiveSettingsKeys
{
    /// <summary>
    /// The key of the setting defining the contact email sent to the Cover Art Archive API.
    /// </summary>
    internal const string CONTACT_EMAIL = "ContactEmail";

    /// <summary>
    /// The key of the setting defining the minimum interval between consecutive requests to the Cover Art Archive API, in seconds.
    /// </summary>
    internal const string MINIMUM_REQUEST_INTERVAL_SECONDS = "MinimumRequestIntervalSeconds";
}
