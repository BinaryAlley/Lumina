#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryTypeMetadataProvidersRequest"/>.
/// </summary>
public static class GetLibraryTypeMetadataProvidersRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryTypeMetadataProvidersQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryTypeMetadataProvidersQuery ToQuery(this GetLibraryTypeMetadataProvidersRequest request)
    {
        return new GetLibraryTypeMetadataProvidersQuery(
            request.LibraryType
        );
    }
}
