#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Plugins;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Plugins;

/// <summary>
/// Fixture class for the <see cref="GetLibraryTypeArtworkProvidersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryTypeArtworkProvidersRequest"/>.
    /// </summary>
    /// <param name="libraryType">Optional. The media library type whose available artwork providers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryTypeArtworkProvidersRequest"/>.</returns>
    public GetLibraryTypeArtworkProvidersRequest Create(string? libraryType = null)
    {
        return new GetLibraryTypeArtworkProvidersRequest(libraryType ?? "Book");
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryTypeArtworkProvidersRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryTypeArtworkProvidersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
