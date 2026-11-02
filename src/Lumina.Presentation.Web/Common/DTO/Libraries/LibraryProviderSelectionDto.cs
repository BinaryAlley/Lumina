#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Libraries;

/// <summary>
/// Data transfer object for the choice made for one metadata provider, artwork provider or book reader of a media library.
/// </summary>
[DebuggerDisplay("PluginId: {PluginId}; IsEnabled: {IsEnabled}")]
public class LibraryProviderSelectionDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin providing the metadata provider, artwork provider or book reader.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets whether the provider or reader is enabled for the media library. Optional, the stored state is kept when absent, and a provider or reader without a stored state starts disabled.
    /// </summary>
    public bool? IsEnabled { get; set; }
}
