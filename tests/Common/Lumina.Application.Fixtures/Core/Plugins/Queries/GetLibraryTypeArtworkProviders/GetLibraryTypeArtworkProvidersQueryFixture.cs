#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;

/// <summary>
/// Fixture class for the <see cref="GetLibraryTypeArtworkProvidersQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryTypeArtworkProvidersQuery"/>.
    /// </summary>
    /// <param name="libraryType">Optional. The media library type whose available artwork providers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryTypeArtworkProvidersQuery"/>.</returns>
    public GetLibraryTypeArtworkProvidersQuery Create(string? libraryType = null)
    {
        return new GetLibraryTypeArtworkProvidersQuery(libraryType ?? "Book");
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryTypeArtworkProvidersQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryTypeArtworkProvidersQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
