#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Plugins;

/// <summary>
/// Fixture class for the <see cref="LibraryArtworkProviderResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryArtworkProviderResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryArtworkProviderResponse"/>.
    /// </summary>
    /// <param name="pluginId">Optional. The Id of the plugin providing the artwork.</param>
    /// <param name="name">Optional. The display name of the artwork provider.</param>
    /// <param name="isEnabled">Optional. Whether the artwork provider is enabled for the media library.</param>
    /// <param name="rank">Optional. The rank of the artwork provider.</param>
    /// <returns>The created <see cref="LibraryArtworkProviderResponse"/>.</returns>
    public LibraryArtworkProviderResponse Create(
        Guid? pluginId = null,
        string? name = null,
        bool? isEnabled = null,
        int? rank = null)
    {
        return new LibraryArtworkProviderResponse(
            pluginId ?? Guid.NewGuid(),
            name ?? _faker.Company.CompanyName(),
            isEnabled ?? _faker.Random.Bool(),
            rank ?? _faker.Random.Int(1, 100));
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryArtworkProviderResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryArtworkProviderResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
