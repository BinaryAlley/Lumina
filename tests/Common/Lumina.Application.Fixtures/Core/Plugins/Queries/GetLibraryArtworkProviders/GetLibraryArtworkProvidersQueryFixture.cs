#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryArtworkProviders;

/// <summary>
/// Fixture class for the <see cref="GetLibraryArtworkProvidersQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryArtworkProvidersQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the artwork providers of a media library.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose artwork providers are retrieved.</param>
    /// <returns>The created query.</returns>
    public GetLibraryArtworkProvidersQuery Create(
        Guid? libraryId = null)
    {
        return new Faker<GetLibraryArtworkProvidersQuery>()
            .CustomInstantiator(f => new GetLibraryArtworkProvidersQuery(
                libraryId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryArtworkProvidersQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryArtworkProvidersQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
