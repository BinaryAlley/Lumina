#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;

/// <summary>
/// Fixture class for the <see cref="GetLibraryTypeMetadataProvidersQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeMetadataProvidersQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryTypeMetadataProvidersQuery"/>.
    /// </summary>
    /// <param name="libraryType">Optional. The media library type whose available metadata providers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryTypeMetadataProvidersQuery"/>.</returns>
    public GetLibraryTypeMetadataProvidersQuery Create(string? libraryType = null)
    {
        return new GetLibraryTypeMetadataProvidersQuery(libraryType ?? "Book");
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryTypeMetadataProvidersQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryTypeMetadataProvidersQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
