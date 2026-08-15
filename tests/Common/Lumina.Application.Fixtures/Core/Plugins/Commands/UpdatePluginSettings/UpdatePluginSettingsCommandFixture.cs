#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Commands.UpdatePluginSettings;

/// <summary>
/// Fixture class for the <see cref="UpdatePluginSettingsCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsCommandFixture
{
    /// <summary>
    /// Creates a random valid command to update the settings of a plugin.
    /// </summary>
    /// <param name="pluginId">Optional. The Id of the plugin.</param>
    /// <param name="settings">Optional. The settings of the plugin.</param>
    /// <returns>The created command.</returns>
    public UpdatePluginSettingsCommand Create(Guid? pluginId = null, IReadOnlyDictionary<string, string>? settings = null)
    {
        return new Faker<UpdatePluginSettingsCommand>()
            .CustomInstantiator(f => new UpdatePluginSettingsCommand(
                default,
                default!))
            .RuleFor(x => x.PluginId, pluginId ?? Guid.NewGuid())
            .RuleFor(x => x.Settings, f => settings ?? new Dictionary<string, string> { [f.Lorem.Word()] = f.Lorem.Word() })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UpdatePluginSettingsCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdatePluginSettingsCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
