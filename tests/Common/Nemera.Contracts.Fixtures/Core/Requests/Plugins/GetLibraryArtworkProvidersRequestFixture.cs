#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Plugins;

/// <summary>
/// Fixture class for the <see cref="GetLibraryArtworkProvidersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryArtworkProvidersRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetLibraryArtworkProvidersRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose artwork providers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryArtworkProvidersRequest"/>.</returns>
    public GetLibraryArtworkProvidersRequest Create(
        Guid? libraryId = null)
    {
        return new GetLibraryArtworkProvidersRequest(
            libraryId ?? _faker.Random.Guid()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryArtworkProvidersRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryArtworkProvidersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
