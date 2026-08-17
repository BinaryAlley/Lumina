#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Plugins;

/// <summary>
/// Represents a request to enable or disable a metadata provider for a media library.
/// </summary>
[DebuggerDisplay("LibraryId: {LibraryId}, PluginId: {PluginId}")]
public class SetLibraryMetadataProviderEnabledRequest
{
    /// <summary>
    /// Gets or sets the Id of the media library whose metadata provider is enabled or disabled.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the plugin providing the metadata.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets whether the metadata provider should be enabled for the media library.
    /// </summary>
    public bool IsEnabled { get; set; }
}
