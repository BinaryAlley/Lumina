#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Responses.Plugins;

/// <summary>
/// Represents an artwork provider configured for a media library.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin providing the artwork.</param>
/// <param name="Name">The display name of the artwork provider.</param>
/// <param name="IsEnabled">Whether the artwork provider is enabled for the media library.</param>
/// <param name="Rank">The rank of the artwork provider, determining the order in which providers are tried.</param>
public sealed record LibraryArtworkProviderResponse(
    Guid PluginId,
    string Name,
    bool IsEnabled,
    int Rank
);
