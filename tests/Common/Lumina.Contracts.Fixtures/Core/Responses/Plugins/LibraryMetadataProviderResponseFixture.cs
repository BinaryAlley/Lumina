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
/// Fixture class for the <see cref="LibraryMetadataProviderResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMetadataProviderResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryMetadataProviderResponse"/>.
    /// </summary>
    /// <param name="pluginId">Optional. The Id of the plugin providing the metadata.</param>
    /// <param name="name">Optional. The display name of the metadata provider.</param>
    /// <param name="isEnabled">Optional. Whether the metadata provider is enabled for the media library.</param>
    /// <param name="rank">Optional. The rank of the metadata provider.</param>
    /// <returns>The created <see cref="LibraryMetadataProviderResponse"/>.</returns>
    public LibraryMetadataProviderResponse Create(
        Guid? pluginId = null,
        string? name = null,
        bool? isEnabled = null,
        int? rank = null)
    {
        return new LibraryMetadataProviderResponse(
            pluginId ?? Guid.NewGuid(),
            name ?? _faker.Company.CompanyName(),
            isEnabled ?? _faker.Random.Bool(),
            rank ?? _faker.Random.Int(1, 100));
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryMetadataProviderResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryMetadataProviderResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
