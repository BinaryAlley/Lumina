#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Plugins;

/// <summary>
/// Represents a request to enable or disable a book reader for a media library.
/// </summary>
[DebuggerDisplay("LibraryId: {LibraryId}, PluginId: {PluginId}")]
public class SetBookReaderEnabledRequest
{
    /// <summary>
    /// Gets or sets the Id of the media library whose book reader is enabled or disabled. Required.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the plugin providing the book reader. Required.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets whether the book reader should be enabled for the media library. Required.
    /// </summary>
    public bool IsEnabled { get; set; }
}
