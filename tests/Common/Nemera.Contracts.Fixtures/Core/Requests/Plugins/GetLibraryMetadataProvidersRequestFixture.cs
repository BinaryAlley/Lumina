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
/// Fixture class for the <see cref="GetLibraryMetadataProvidersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryMetadataProvidersRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetLibraryMetadataProvidersRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose metadata providers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryMetadataProvidersRequest"/>.</returns>
    public GetLibraryMetadataProvidersRequest Create(Guid? libraryId = null)
    {
        return new GetLibraryMetadataProvidersRequest(
            libraryId ?? _faker.Random.Guid()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryMetadataProvidersRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryMetadataProvidersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
