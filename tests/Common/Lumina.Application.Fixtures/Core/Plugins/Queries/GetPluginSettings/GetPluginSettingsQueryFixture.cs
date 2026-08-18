#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Plugins.Queries.GetPluginSettings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Queries.GetPluginSettings;

/// <summary>
/// Fixture class for the <see cref="GetPluginSettingsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the settings of a plugin.
    /// </summary>
    /// <param name="pluginId">Optional. The unique identifier of the plugin.</param>
    /// <returns>The created query.</returns>
    public GetPluginSettingsQuery Create(Guid? pluginId = null)
    {
        return new Faker<GetPluginSettingsQuery>()
            .CustomInstantiator(f => new GetPluginSettingsQuery(
                pluginId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetPluginSettingsQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetPluginSettingsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
