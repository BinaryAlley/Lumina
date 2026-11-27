#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Responses.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="LibraryArtworkProviderConfigurationEntity"/>.
/// </summary>
public static class LibraryArtworkProviderConfigurationEntityMapping
{
    /// <summary>
    /// Converts <paramref name="configuration"/> to <see cref="LibraryArtworkProviderResponse"/>.
    /// </summary>
    /// <param name="configuration">The repository entity to be converted.</param>
    /// <param name="providerName">The display name of the artwork provider.</param>
    /// <returns>The converted response.</returns>
    public static LibraryArtworkProviderResponse ToResponse(this LibraryArtworkProviderConfigurationEntity configuration, string providerName)
    {
        return new LibraryArtworkProviderResponse(
            PluginId: configuration.PluginId,
            Name: providerName,
            IsEnabled: configuration.IsEnabled,
            Rank: configuration.Rank
        );
    }
}
