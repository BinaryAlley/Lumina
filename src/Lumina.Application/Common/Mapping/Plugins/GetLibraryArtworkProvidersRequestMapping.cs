#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryArtworkProvidersRequest"/>.
/// </summary>
public static class GetLibraryArtworkProvidersRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryArtworkProvidersQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryArtworkProvidersQuery ToQuery(this GetLibraryArtworkProvidersRequest request)
    {
        return new GetLibraryArtworkProvidersQuery(LibraryId: request.LibraryId);
    }
}
