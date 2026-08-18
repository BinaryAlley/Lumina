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
/// Fixture class for the <see cref="GetPluginSettingsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetPluginSettingsRequest"/>.
    /// </summary>
    /// <param name="pluginId">Optional. The Id of the plugin whose settings are retrieved.</param>
    /// <returns>The created <see cref="GetPluginSettingsRequest"/>.</returns>
    public GetPluginSettingsRequest Create(Guid? pluginId = null)
    {
        return new GetPluginSettingsRequest(
            pluginId ?? _faker.Random.Guid()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetPluginSettingsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetPluginSettingsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
