#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;

/// <summary>
/// Fixture class for the <see cref="LibraryMetadataProviderConfigurationEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMetadataProviderConfigurationEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryMetadataProviderConfigurationEntity"/> for the provided media library, plugin and rank.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the configuration belongs to.</param>
    /// <param name="pluginId">The Id of the plugin providing the metadata.</param>
    /// <param name="rank">The rank of the metadata provider, determining the order in which providers are tried.</param>
    /// <param name="isEnabled">Whether the metadata provider is enabled or not.</param>
    /// <returns>The created <see cref="LibraryMetadataProviderConfigurationEntity"/>.</returns>
    public LibraryMetadataProviderConfigurationEntity Create(
        Guid libraryId, 
        Guid pluginId, 
        int rank, 
        bool isEnabled = true)
    {
        return new Faker<LibraryMetadataProviderConfigurationEntity>()
            .RuleFor(configuration => configuration.Id, faker => faker.Random.Guid())
            .RuleFor(configuration => configuration.LibraryId, libraryId)
            .RuleFor(configuration => configuration.PluginId, pluginId)
            .RuleFor(configuration => configuration.IsEnabled, isEnabled)
            .RuleFor(configuration => configuration.Rank, rank)
            .RuleFor(configuration => configuration.CreatedOnUtc, faker => faker.Date.Past())
            .RuleFor(configuration => configuration.CreatedBy, faker => faker.Random.Guid())
            .RuleFor(configuration => configuration.UpdatedOnUtc, faker => faker.Date.Recent())
            .RuleFor(configuration => configuration.UpdatedBy, faker => faker.Random.Guid())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryMetadataProviderConfigurationEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <param name="libraryId">The Id of the media library the configurations belong to.</param>
    /// <param name="pluginId">The Id of the plugin providing the metadata.</param>
    /// <param name="rank">The rank of the metadata providers.</param>
    /// <returns>List of configured <see cref="LibraryMetadataProviderConfigurationEntity"/> instances.</returns>
    public List<LibraryMetadataProviderConfigurationEntity> CreateMany(int count, Guid libraryId, Guid pluginId, int rank)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create(libraryId, pluginId, rank))];
    }
}
