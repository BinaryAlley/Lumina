#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Plugins;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Plugins;

/// <summary>
/// Fixture class for the <see cref="GetLibraryTypeMetadataProvidersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeMetadataProvidersRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryTypeMetadataProvidersRequest"/>.
    /// </summary>
    /// <param name="libraryType">Optional. The media library type whose available metadata providers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryTypeMetadataProvidersRequest"/>.</returns>
    public GetLibraryTypeMetadataProvidersRequest Create(string? libraryType = null)
    {
        return new GetLibraryTypeMetadataProvidersRequest(libraryType ?? "Book");
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryTypeMetadataProvidersRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryTypeMetadataProvidersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
