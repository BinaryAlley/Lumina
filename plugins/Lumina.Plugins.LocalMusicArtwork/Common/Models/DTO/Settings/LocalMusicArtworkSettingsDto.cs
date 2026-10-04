namespace Lumina.Plugins.LocalMusicArtwork.Common.Models.DTO.Settings;

/// <summary>
/// Data transfer object for the settings that configure the local music artwork plugin.
/// </summary>
internal sealed class LocalMusicArtworkSettingsDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the cover embedded in the audio files is extracted when the album has no cover image on disk.
    /// Defaults to <see langword="true"/>, so that a cover is found even when the library stores none on disk.
    /// </summary>
    public bool ShouldExtractEmbeddedCover { get; set; } = true;
}
