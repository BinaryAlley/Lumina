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
/// Fixture class for the <see cref="UpdatePluginSettingsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdatePluginSettingsRequest"/>.
    /// </summary>
    /// <param name="pluginId">Optional. The Id of the plugin whose settings are updated.</param>
    /// <param name="settings">Optional. The settings to apply to the plugin.</param>
    /// <returns>The created <see cref="UpdatePluginSettingsRequest"/>.</returns>
    public UpdatePluginSettingsRequest Create(
        Guid? pluginId = null,
        IReadOnlyDictionary<string, string>? settings = null)
    {
        return new UpdatePluginSettingsRequest(
            pluginId ?? _faker.Random.Guid(),
            settings ?? new Dictionary<string, string>
            {
                [_faker.Lorem.Word()] = _faker.Lorem.Word()
            }
        );
    }

    /// <summary>
    /// Creates a list of <see cref="UpdatePluginSettingsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdatePluginSettingsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
