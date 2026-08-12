#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Responses.Plugins;

/// <summary>
/// Represents a metadata provider configured for a media library.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin providing the metadata.</param>
/// <param name="Name">The display name of the metadata provider.</param>
/// <param name="IsEnabled">Whether the metadata provider is enabled for the media library.</param>
/// <param name="Rank">The rank of the metadata provider, determining the order in which providers are tried.</param>
public sealed record LibraryMetadataProviderResponse(
    Guid PluginId,
    string Name,
    bool IsEnabled,
    int Rank
);
