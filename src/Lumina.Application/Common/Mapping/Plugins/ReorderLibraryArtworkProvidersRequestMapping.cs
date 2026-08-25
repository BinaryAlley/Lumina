#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="ReorderLibraryArtworkProvidersRequest"/>.
/// </summary>
public static class ReorderLibraryArtworkProvidersRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ReorderLibraryArtworkProvidersCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static ReorderLibraryArtworkProvidersCommand ToCommand(this ReorderLibraryArtworkProvidersRequest request)
    {
        return new ReorderLibraryArtworkProvidersCommand(
            LibraryId: request.LibraryId,
            PluginIds: request.PluginIds
        );
    }
}
