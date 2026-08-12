#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryMetadataProvidersRequest"/>.
/// </summary>
public static class GetLibraryMetadataProvidersRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryMetadataProvidersQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryMetadataProvidersQuery ToQuery(this GetLibraryMetadataProvidersRequest request)
    {
        return new GetLibraryMetadataProvidersQuery(request.LibraryId);
    }
}
