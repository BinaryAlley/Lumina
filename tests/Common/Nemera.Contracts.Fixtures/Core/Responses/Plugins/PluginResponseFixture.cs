#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Plugins;

/// <summary>
/// Fixture class for the <see cref="PluginResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PluginResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the plugin.</param>
    /// <param name="name">Optional. The display name of the plugin.</param>
    /// <param name="author">Optional. The author of the plugin.</param>
    /// <param name="version">Optional. The version of the plugin.</param>
    /// <param name="description">Optional. The description of the plugin.</param>
    /// <param name="loadStatus">Optional. The load status of the plugin.</param>
    /// <param name="loadError">Optional. The error message when the plugin failed to load.</param>
    /// <param name="settings">Optional. The settings of the plugin.</param>
    /// <returns>The created <see cref="PluginResponse"/>.</returns>
    public PluginResponse Create(
        Guid? id = null,
        string? name = null,
        string? author = null,
        string? version = null,
        string? description = null,
        PluginLoadStatus? loadStatus = null,
        string? loadError = null,
        IReadOnlyDictionary<string, string>? settings = null)
    {
        return new PluginResponse(
            id ?? Guid.NewGuid(),
            name ?? _faker.Commerce.ProductName(),
            author ?? _faker.Company.CompanyName(),
            version ?? _faker.System.Semver(),
            description ?? _faker.Lorem.Sentence(),
            loadStatus ?? _faker.PickRandom<PluginLoadStatus>(),
            loadError,
            settings ?? new Dictionary<string, string>
            {
                [_faker.Lorem.Word()] = _faker.Lorem.Word()
            }
        );
    }

    /// <summary>
    /// Creates a list of <see cref="PluginResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PluginResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
