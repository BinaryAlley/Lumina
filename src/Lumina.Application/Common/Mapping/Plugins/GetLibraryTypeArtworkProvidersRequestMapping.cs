#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryTypeArtworkProvidersRequest"/>.
/// </summary>
public static class GetLibraryTypeArtworkProvidersRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryTypeArtworkProvidersQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryTypeArtworkProvidersQuery ToQuery(this GetLibraryTypeArtworkProvidersRequest request)
    {
        return new GetLibraryTypeArtworkProvidersQuery(
            request.LibraryType
        );
    }
}
