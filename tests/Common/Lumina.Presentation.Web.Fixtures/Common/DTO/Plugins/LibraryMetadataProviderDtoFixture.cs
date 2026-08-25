#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;

/// <summary>
/// Fixture class for generating <see cref="LibraryMetadataProviderDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMetadataProviderDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="LibraryMetadataProviderDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="pluginId">Optional unique identifier of the plugin providing the metadata.</param>
    /// <param name="name">Optional display name of the metadata provider.</param>
    /// <param name="isEnabled">Whether the metadata provider is enabled for the media library or not.</param>
    /// <param name="rank">Optional rank of the metadata provider, determining the order in which providers are tried.</param>
    /// <returns>A configured <see cref="LibraryMetadataProviderDto"/> instance.</returns>
    public LibraryMetadataProviderDto Create(
        Guid? pluginId = null, 
        string? name = null, 
        bool? isEnabled = null, 
        int? rank = null)
    {
        return new LibraryMetadataProviderDto
        {
            PluginId = pluginId ?? Guid.NewGuid(),
            Name = name ?? _faker.Company.CompanyName(),
            IsEnabled = isEnabled ?? _faker.Random.Bool(),
            Rank = rank ?? _faker.Random.Int(0, 100)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryMetadataProviderDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryMetadataProviderDto"/> instances.</returns>
    public List<LibraryMetadataProviderDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
