namespace Lumina.Plugins.LocalMusicArtwork.Core.Settings;

/// <summary>
/// Keys of the settings of the local music artwork plugin, shared by the settings schema and the settings provider.
/// </summary>
internal static class LocalMusicArtworkSettingsKeys
{
    /// <summary>
    /// The key of the setting that controls whether the cover embedded in the audio files is extracted when the album has no cover image on disk.
    /// </summary>
    internal const string SHOULD_EXTRACT_EMBEDDED_COVER = "ShouldExtractEmbeddedCover";
}
