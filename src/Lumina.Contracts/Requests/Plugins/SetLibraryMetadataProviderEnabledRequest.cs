#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to enable or disable a metadata provider for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose metadata provider is enabled or disabled. Required.</param>
/// <param name="PluginId">The unique identifier of the plugin providing the metadata. Required.</param>
/// <param name="IsEnabled">Whether the metadata provider should be enabled for the media library.</param>
[DebuggerDisplay("LibraryId: {LibraryId}, PluginId: {PluginId}")]
public sealed record SetLibraryMetadataProviderEnabledRequest(
    Guid LibraryId,
    Guid PluginId,
    bool IsEnabled
);
