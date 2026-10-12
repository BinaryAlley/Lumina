#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Plugins;

/// <summary>
/// Represents a request to enable or disable an artwork provider for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose artwork provider is enabled or disabled. Required.</param>
/// <param name="PluginId">The unique identifier of the plugin providing the artwork. Required.</param>
/// <param name="IsEnabled">Whether the artwork provider should be enabled for the media library. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}, PluginId: {PluginId}")]
public record SetLibraryArtworkProviderEnabledRequest(
    Guid LibraryId,
    Guid PluginId,
    bool IsEnabled
);
