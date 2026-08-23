#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Enums.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;

/// <summary>
/// Fixture class for generating <see cref="PluginDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PluginDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional identifier of the plugin.</param>
    /// <param name="name">Optional name of the plugin.</param>
    /// <param name="author">Optional author of the plugin.</param>
    /// <param name="version">Optional version of the plugin.</param>
    /// <param name="loadStatus">Optional load status of the plugin.</param>
    /// <returns>A configured <see cref="PluginDto"/> instance.</returns>
    public PluginDto Create(Guid? id = null, string? name = null, string? author = null, string? version = null, PluginLoadStatus? loadStatus = null)
    {
        return new PluginDto
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? _faker.Hacker.Noun(),
            Author = author ?? _faker.Name.FullName(),
            Version = version ?? _faker.System.Semver(),
            Description = _faker.Lorem.Sentence(),
            LoadStatus = loadStatus ?? PluginLoadStatus.Loaded,
            LoadError = null,
            Settings = null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="PluginDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PluginDto"/> instances.</returns>
    public List<PluginDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
