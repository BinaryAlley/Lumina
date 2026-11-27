#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents the choice made for one metadata provider, artwork provider or book reader of a media library.
/// </summary>
/// <param name="PluginId">The Id of the plugin providing the metadata provider, artwork provider or book reader. Required.</param>
/// <param name="IsEnabled">Whether the provider or reader is enabled for the media library. Optional, the stored state is kept when absent, and a provider or reader without a stored state starts disabled.</param>
[DebuggerDisplay("PluginId: {PluginId}; IsEnabled: {IsEnabled}")]
public record LibraryProviderSelectionRequest(
    Guid PluginId,
    bool? IsEnabled
);
