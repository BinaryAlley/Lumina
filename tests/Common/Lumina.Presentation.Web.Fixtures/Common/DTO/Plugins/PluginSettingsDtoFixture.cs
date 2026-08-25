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
/// Fixture class for generating <see cref="PluginSettingsDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingsDtoFixture
{
    private readonly Faker _faker = new();
    private readonly PluginSettingDescriptorDtoFixture _pluginSettingDescriptorDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="PluginSettingsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="pluginId">Optional unique identifier of the plugin.</param>
    /// <param name="includeSettings">Whether to populate the current settings values, or leave them null.</param>
    /// <param name="includeSchema">Whether to populate the settings schema, or leave it empty.</param>
    /// <returns>A configured <see cref="PluginSettingsDto"/> instance.</returns>
    public PluginSettingsDto Create(
        Guid? pluginId = null, 
        bool includeSettings = true, 
        bool includeSchema = true)
    {
        string settingKey = _faker.Lorem.Word();
        return new PluginSettingsDto
        {
            PluginId = pluginId ?? Guid.NewGuid(),
            Schema = includeSchema
                ? [_pluginSettingDescriptorDtoFixture.Create(key: settingKey)]
                : [],
            Settings = includeSettings
                ? new Dictionary<string, string> { [settingKey] = _faker.Lorem.Word() }
                : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="PluginSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PluginSettingsDto"/> instances.</returns>
    public List<PluginSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
