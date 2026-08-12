#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Plugins;

/// <summary>
/// Represents a metadata provider configured for a media library.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class LibraryMetadataProviderModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin providing the metadata.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the metadata provider.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the metadata provider is enabled for the media library.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the rank of the metadata provider, determining the order in which providers are tried.
    /// </summary>
    public int Rank { get; set; }
}
