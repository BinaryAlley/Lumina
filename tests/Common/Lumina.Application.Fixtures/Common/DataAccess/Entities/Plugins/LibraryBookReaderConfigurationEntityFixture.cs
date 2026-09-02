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
/// Fixture class for the <see cref="LibraryBookReaderConfigurationEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryBookReaderConfigurationEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryBookReaderConfigurationEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the configuration.</param>
    /// <param name="libraryId">Optional. The Id of the media library the configuration belongs to.</param>
    /// <param name="pluginId">Optional. The Id of the plugin providing the book reader.</param>
    /// <param name="isEnabled">Optional. Whether the book reader is enabled for the media library.</param>
    /// <returns>The created <see cref="LibraryBookReaderConfigurationEntity"/>.</returns>
    public LibraryBookReaderConfigurationEntity Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? pluginId = null,
        bool? isEnabled = null)
    {
        return new Faker<LibraryBookReaderConfigurationEntity>()
            .RuleFor(configuration => configuration.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(configuration => configuration.LibraryId, _ => libraryId ?? Guid.NewGuid())
            .RuleFor(configuration => configuration.PluginId, _ => pluginId ?? Guid.NewGuid())
            .RuleFor(configuration => configuration.IsEnabled, _ => isEnabled ?? true)
            .RuleFor(configuration => configuration.CreatedOnUtc, faker => faker.Date.Past())
            .RuleFor(configuration => configuration.CreatedBy, faker => faker.Random.Guid())
            .RuleFor(configuration => configuration.UpdatedOnUtc, faker => faker.Date.Recent())
            .RuleFor(configuration => configuration.UpdatedBy, faker => faker.Random.Guid())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryBookReaderConfigurationEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryBookReaderConfigurationEntity"/> instances.</returns>
    public List<LibraryBookReaderConfigurationEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
