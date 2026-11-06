#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.Management;

/// <summary>
/// Data transfer object for the choice made for one metadata provider, artwork provider or book reader of a media library, carried by the commands that create or update a media library.
/// </summary>
/// <param name="PluginId">The Id of the plugin providing the metadata provider, artwork provider or book reader.</param>
/// <param name="IsEnabled">Whether the provider or reader is enabled for the media library. When absent, the stored state is kept, and a provider or reader without a stored state starts disabled.</param>
[DebuggerDisplay("PluginId: {PluginId}; IsEnabled: {IsEnabled}")]
public record LibraryProviderSelectionDto(
    Guid PluginId,
    bool? IsEnabled
);
