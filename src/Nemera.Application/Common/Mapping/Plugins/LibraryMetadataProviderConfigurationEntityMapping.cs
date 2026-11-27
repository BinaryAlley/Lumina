#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Responses.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="LibraryMetadataProviderConfigurationEntity"/>.
/// </summary>
public static class LibraryMetadataProviderConfigurationEntityMapping
{
    /// <summary>
    /// Converts <paramref name="configuration"/> to <see cref="LibraryMetadataProviderResponse"/>.
    /// </summary>
    /// <param name="configuration">The repository entity to be converted.</param>
    /// <param name="providerName">The display name of the metadata provider.</param>
    /// <returns>The converted response.</returns>
    public static LibraryMetadataProviderResponse ToResponse(this LibraryMetadataProviderConfigurationEntity configuration, string providerName)
    {
        return new LibraryMetadataProviderResponse(
            configuration.PluginId,
            providerName,
            configuration.IsEnabled,
            configuration.Rank
        );
    }
}
