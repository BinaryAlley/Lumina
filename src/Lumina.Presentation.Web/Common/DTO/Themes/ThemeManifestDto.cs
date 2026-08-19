#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the theme.json manifest of a theme package.
/// </summary>
[DebuggerDisplay("Id: {Id}, Name: {Name}")]
public sealed class ThemeManifestDto
{
    /// <summary>
    /// Gets or sets the supported schema version of the manifest.
    /// </summary>
    public int SchemaVersion { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the theme, a lowercase kebab-case value.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the theme.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the theme.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the theme.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the theme, using semantic version form.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preview image path relative to the theme package root, or <see langword="null"/> when the theme has no preview.
    /// </summary>
    public string? Preview { get; set; }

    /// <summary>
    /// Gets or sets the template mappings from template key to an HTML file path under templates/.
    /// </summary>
    public Dictionary<string, string> Templates { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
