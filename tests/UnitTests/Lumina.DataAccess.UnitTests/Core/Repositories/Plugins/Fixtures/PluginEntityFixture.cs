#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Plugins.Fixtures;

/// <summary>
/// Fixture class for the <see cref="PluginEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="PluginEntity"/>.
    /// </summary>
    /// <param name="id">The Id of the plugin, or <see langword="null"/> to generate a random one.</param>
    /// <returns>The created <see cref="PluginEntity"/>.</returns>
    public PluginEntity CreatePluginEntity(Guid? id = null)
    {
        return new Faker<PluginEntity>()
            .RuleFor(plugin => plugin.Id, faker => id ?? faker.Random.Guid())
            .RuleFor(plugin => plugin.Name, faker => faker.Company.CompanyName())
            .RuleFor(plugin => plugin.Author, faker => faker.Name.FullName())
            .RuleFor(plugin => plugin.Version, faker => faker.System.Semver())
            .RuleFor(plugin => plugin.Description, faker => faker.Lorem.Sentence())
            .RuleFor(plugin => plugin.LoadStatus, faker => faker.PickRandom<PluginLoadStatus>())
            .RuleFor(plugin => plugin.SettingsJson, faker => JsonSerializer.Serialize(new Dictionary<string, string> { [faker.Random.String2(1, 10)] = faker.Random.String2(1, 20) }))
            .RuleFor(plugin => plugin.CreatedOnUtc, faker => faker.Date.Past())
            .RuleFor(plugin => plugin.CreatedBy, faker => faker.Random.Guid())
            .RuleFor(plugin => plugin.UpdatedOnUtc, faker => faker.Date.Recent())
            .RuleFor(plugin => plugin.UpdatedBy, faker => faker.Random.Guid())
            .Generate();
    }
}
