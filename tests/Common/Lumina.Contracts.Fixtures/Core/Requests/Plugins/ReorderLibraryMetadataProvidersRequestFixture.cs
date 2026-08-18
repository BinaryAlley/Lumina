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
/// Fixture class for the <see cref="ReorderLibraryMetadataProvidersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ReorderLibraryMetadataProvidersRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose metadata providers are reordered.</param>
    /// <param name="pluginIds">Optional. The plugin Ids in the new order.</param>
    /// <returns>The created <see cref="ReorderLibraryMetadataProvidersRequest"/>.</returns>
    public ReorderLibraryMetadataProvidersRequest Create(
        Guid? libraryId = null,
        IReadOnlyList<Guid>? pluginIds = null)
    {
        return new ReorderLibraryMetadataProvidersRequest(
            libraryId ?? _faker.Random.Guid(),
            pluginIds ?? [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ReorderLibraryMetadataProvidersRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReorderLibraryMetadataProvidersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
