#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;

/// <summary>
/// Fixture class for generating <see cref="UpdatePluginSettingsRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="UpdatePluginSettingsRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="pluginId">Optional identifier of the plugin.</param>
    /// <param name="settings">Optional dictionary of settings.</param>
    /// <returns>A configured <see cref="UpdatePluginSettingsRequest"/> instance.</returns>
    public UpdatePluginSettingsRequest Create(Guid? pluginId = null, Dictionary<string, string>? settings = null)
    {
        Faker faker = new();
        return new UpdatePluginSettingsRequest
        {
            PluginId = pluginId ?? Guid.NewGuid(),
            Settings = settings ?? new Dictionary<string, string> { [faker.Lorem.Word()] = faker.Lorem.Word() }
        };
    }

    /// <summary>
    /// Creates multiple <see cref="UpdatePluginSettingsRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdatePluginSettingsRequest"/> instances.</returns>
    public List<UpdatePluginSettingsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
