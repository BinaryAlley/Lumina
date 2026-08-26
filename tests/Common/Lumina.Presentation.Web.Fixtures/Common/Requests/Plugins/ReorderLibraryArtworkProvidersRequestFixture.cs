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
/// Fixture class for generating <see cref="ReorderLibraryArtworkProvidersRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryArtworkProvidersRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ReorderLibraryArtworkProvidersRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="libraryId">Optional identifier of the media library.</param>
    /// <param name="pluginIds">Optional plugin Ids in the new order.</param>
    /// <returns>A configured <see cref="ReorderLibraryArtworkProvidersRequest"/> instance.</returns>
    public ReorderLibraryArtworkProvidersRequest Create(
        Guid? libraryId = null,
        List<Guid>? pluginIds = null)
    {
        return new ReorderLibraryArtworkProvidersRequest
        {
            LibraryId = libraryId ?? Guid.NewGuid(),
            PluginIds = pluginIds ?? [_faker.Random.Guid(), _faker.Random.Guid()]
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ReorderLibraryArtworkProvidersRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReorderLibraryArtworkProvidersRequest"/> instances.</returns>
    public List<ReorderLibraryArtworkProvidersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
