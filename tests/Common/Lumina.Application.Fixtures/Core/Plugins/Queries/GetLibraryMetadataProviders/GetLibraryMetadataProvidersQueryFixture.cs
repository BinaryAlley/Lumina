#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryMetadataProviders;

/// <summary>
/// Fixture class for the <see cref="GetLibraryMetadataProvidersQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryMetadataProvidersQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the metadata providers of a media library.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose metadata providers are retrieved.</param>
    /// <returns>The created query.</returns>
    public GetLibraryMetadataProvidersQuery Create(Guid? libraryId = null)
    {
        return new Faker<GetLibraryMetadataProvidersQuery>()
            .CustomInstantiator(f => new GetLibraryMetadataProvidersQuery(
                libraryId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryMetadataProvidersQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryMetadataProvidersQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
