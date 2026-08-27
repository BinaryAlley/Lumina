#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Plugins;

/// <summary>
/// Data transfer object for an artwork provider configured for a media library.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class LibraryArtworkProviderDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin providing the artwork.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the artwork provider.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the artwork provider is enabled for the media library.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the rank of the artwork provider, determining the order in which providers are tried.
    /// </summary>
    public int Rank { get; set; }
}
