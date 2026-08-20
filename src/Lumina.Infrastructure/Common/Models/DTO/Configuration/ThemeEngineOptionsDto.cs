#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing theme engine settings.
/// </summary>
[DebuggerDisplay("Section name: {SECTION_NAME}")]
public sealed class ThemeEngineOptionsDto
{
    public const string SECTION_NAME = "ThemeEngine";

    /// <summary>
    /// Gets or sets the directory where installed theme packs are stored, relative to the API base directory when not rooted.
    /// </summary>
    public string StoragePath { get; set; } = "themes";

    /// <summary>
    /// Gets or sets the directory where the theme pack archives shipped with the application are located, relative to the API base directory when not rooted.
    /// </summary>
    public string BundledThemesPath { get; set; } = "themes/bundled";

    /// <summary>
    /// Gets or sets the identifier of the theme selected when no valid current theme is persisted.
    /// </summary>
    public string DefaultThemeId { get; set; } = "lumina-default";

    /// <summary>
    /// Gets or sets the maximum allowed size of an uploaded theme archive, in bytes.
    /// </summary>
    public long MaxArchiveBytes { get; set; } = 8 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum allowed total size of an extracted theme pack, in bytes.
    /// </summary>
    public long MaxExpandedBytes { get; set; } = 24 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum allowed size of a single file within a theme pack, in bytes.
    /// </summary>
    public long MaxSingleFileBytes { get; set; } = 6 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum allowed number of entries in a theme pack archive.
    /// </summary>
    public int MaxEntries { get; set; } = 250;

    /// <summary>
    /// Gets or sets a value indicating whether script elements in theme templates and script files in theme assets are allowed to be served.
    /// </summary>
    public bool AllowThemeScripts { get; set; }
}
