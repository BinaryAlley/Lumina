#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="ReorderLibraryMetadataProvidersRequest"/>.
/// </summary>
public static class ReorderLibraryMetadataProvidersRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ReorderLibraryMetadataProvidersCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static ReorderLibraryMetadataProvidersCommand ToCommand(this ReorderLibraryMetadataProvidersRequest request)
    {
        return new ReorderLibraryMetadataProvidersCommand(request.LibraryId, request.PluginIds);
    }
}
