#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Responses.Plugins;

/// <summary>
/// Represents an artwork provider, either as configured for a media library, or as available for a library type, the way a new library of that type starts.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin providing the artwork.</param>
/// <param name="Name">The display name of the artwork provider.</param>
/// <param name="IsEnabled">Whether the artwork provider is enabled for the media library. Always <see langword="false"/> for a provider available for a library type, since a new provider starts disabled.</param>
/// <param name="Rank">The rank of the artwork provider, determining the order in which providers are tried. For a provider available for a library type, it is the rank a new library starts with, in alphabetical order of the plugin names.</param>
public sealed record LibraryArtworkProviderResponse(
    Guid PluginId,
    string Name,
    bool IsEnabled,
    int Rank
);
