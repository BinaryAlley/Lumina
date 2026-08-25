#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;

/// <summary>
/// Fixture class for generating <see cref="ReorderLibraryMetadataProvidersRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ReorderLibraryMetadataProvidersRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="libraryId">Optional identifier of the media library.</param>
    /// <param name="pluginIds">Optional plugin Ids in the new order.</param>
    /// <returns>A configured <see cref="ReorderLibraryMetadataProvidersRequest"/> instance.</returns>
    public ReorderLibraryMetadataProvidersRequest Create(
        Guid? libraryId = null,
        List<Guid>? pluginIds = null)
    {
        return new ReorderLibraryMetadataProvidersRequest
        {
            LibraryId = libraryId ?? Guid.NewGuid(),
            PluginIds = pluginIds ?? [_faker.Random.Guid(), _faker.Random.Guid()]
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ReorderLibraryMetadataProvidersRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReorderLibraryMetadataProvidersRequest"/> instances.</returns>
    public List<ReorderLibraryMetadataProvidersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
