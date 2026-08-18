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
/// Fixture class for the <see cref="PluginSettingsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingsResponseFixture
{
    private readonly Faker _faker = new();
    private readonly PluginSettingDescriptorResponseFixture _pluginSettingDescriptorResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="PluginSettingsResponse"/>.
    /// </summary>
    /// <param name="pluginId">Optional. The Id of the plugin.</param>
    /// <param name="schema">Optional. The schema of the plugin settings.</param>
    /// <param name="settings">Optional. The current values of the plugin settings.</param>
    /// <returns>The created <see cref="PluginSettingsResponse"/>.</returns>
    public PluginSettingsResponse Create(
        Guid? pluginId = null,
        IReadOnlyList<PluginSettingDescriptorResponse>? schema = null,
        IReadOnlyDictionary<string, string>? settings = null)
    {
        return new PluginSettingsResponse(
            pluginId ?? Guid.NewGuid(),
            schema ?? _pluginSettingDescriptorResponseFixture.CreateMany(3),
            settings ?? new Dictionary<string, string>
            {
                [_faker.Lorem.Word()] = _faker.Lorem.Word()
            }
        );
    }

    /// <summary>
    /// Creates a list of <see cref="PluginSettingsResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PluginSettingsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
