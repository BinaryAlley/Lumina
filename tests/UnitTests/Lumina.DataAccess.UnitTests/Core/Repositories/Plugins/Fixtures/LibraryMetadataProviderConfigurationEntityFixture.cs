#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Plugins.Fixtures;

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
    /// <returns>The created <see cref="LibraryMetadataProviderConfigurationEntity"/>.</returns>
    public LibraryMetadataProviderConfigurationEntity CreateConfiguration(Guid libraryId, Guid pluginId, int rank)
    {
        return new Faker<LibraryMetadataProviderConfigurationEntity>()
            .RuleFor(configuration => configuration.Id, faker => faker.Random.Guid())
            .RuleFor(configuration => configuration.LibraryId, libraryId)
            .RuleFor(configuration => configuration.PluginId, pluginId)
            .RuleFor(configuration => configuration.IsEnabled, faker => faker.Random.Bool())
            .RuleFor(configuration => configuration.Rank, rank)
            .RuleFor(configuration => configuration.CreatedOnUtc, faker => faker.Date.Past())
            .RuleFor(configuration => configuration.CreatedBy, faker => faker.Random.Guid())
            .RuleFor(configuration => configuration.UpdatedOnUtc, faker => faker.Date.Recent())
            .RuleFor(configuration => configuration.UpdatedBy, faker => faker.Random.Guid())
            .Generate();
    }
}
