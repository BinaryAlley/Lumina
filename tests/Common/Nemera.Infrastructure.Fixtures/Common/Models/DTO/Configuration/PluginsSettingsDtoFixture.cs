#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="PluginsSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginsSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid <see cref="PluginsSettingsDto"/>.
    /// </summary>
    /// <param name="directory">Optional. The directory where the plugin assemblies are located.</param>
    /// <returns>The created <see cref="PluginsSettingsDto"/>.</returns>
    public PluginsSettingsDto Create(string? directory = null)
    {
        return new PluginsSettingsDto
        {
            Directory = directory ?? _faker.System.DirectoryPath()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="PluginsSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PluginsSettingsDto"/> instances.</returns>
    public List<PluginsSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
