#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="SetLibraryArtworkProviderEnabledRequest"/>.
/// </summary>
public static class SetLibraryArtworkProviderEnabledRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="SetLibraryArtworkProviderEnabledCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static SetLibraryArtworkProviderEnabledCommand ToCommand(this SetLibraryArtworkProviderEnabledRequest request)
    {
        return new SetLibraryArtworkProviderEnabledCommand(
            LibraryId: request.LibraryId,
            PluginId: request.PluginId,
            IsEnabled: request.IsEnabled
        );
    }
}
